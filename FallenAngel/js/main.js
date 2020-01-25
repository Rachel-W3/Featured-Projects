"use strict";

// https://stackoverflow.com/questions/12273451/how-to-fix-delay-in-javascript-keydown
let keyState = {};
window.addEventListener('keydown', function(e) {
    keyState[e.keyCode || e.which] = true;
}, true);    
window.addEventListener('keyup', function(e) {
    keyState[e.keyCode || e.which] = false;
}, true);

// Source code: https://jsfiddle.net/bigtimebuddy/oaLwp0p9/
// Try getting application to either fit whole screen or 
// open in a new window resized to its resolution
const app = new PIXI.Application({
    autoResize: true,
    resolution: devicePixelRatio,
    backgroundColor: 0x7491BE
});

document.body.appendChild(app.view);

// Listen for window resize events
window.addEventListener('resize', resize);

// Resize function window
function resize() {
	// Resize the renderer
	app.renderer.resize(window.innerWidth, window.innerHeight);
}

resize();

// constants
const sceneWidth = app.screen.width;
const sceneHeight = app.screen.height;
const minCoolDown = 10;
const attackBarMax = 300;

const angelFrames = [];
for(let i = 0; i < 12; i++) {
    let texture = PIXI.Texture.from(`images/angel/${i}.png`);
    angelFrames.push(texture);
}

const bgImage = PIXI.Texture.from("images/blue-skies-background.jpg");
const sky = new PIXI.TilingSprite(
    bgImage,
    app.screen.width,
    app.screen.height,
);
sky.tilePosition.y = sky.height;
app.stage.addChild(sky);

PIXI.loader.
add(["images/meteor.png"]).
load(gameSetup);

// aliases
let stage;

// game variables
let startScene;
let gameScene, scoreLabel, attackBar, atkDisabled, atkReadyLabel;
let angel, wingSound, shieldSound;
let gameOverScene, gameOverScoreLabel, retryButton, returnHomeButton;

let meteors = [];
let meteorCount;
let spawnCoolDown; // Delay (in frames) between spawning meteors
let elapsedFrames;
let score;
let levelNum;
let paused = true;

function gameSetup() {
    stage = app.stage;

    // Creating start scene
    startScene = new PIXI.Container();
    stage.addChild(startScene);

    // Creating the main `game` scene (invisible at start-up)
    gameScene = new PIXI.Container();
    gameScene.visible = false;
    stage.addChild(gameScene);
    
    // Creating game over scene (also invisible)
    gameOverScene = new PIXI.Container();
    gameOverScene.visible = false;
    stage.addChild(gameOverScene);

    // Making a new Angel instance just for the start scene, because I can't seem
    // to add an instance to multiple containers.
    let fallenAngel = new PIXI.Sprite.from("images/angel/fallen.png");
    fallenAngel.anchor.set(0.5, 0.5);
    fallenAngel.x = sceneWidth / 2;
    fallenAngel.y = sceneHeight - 250;
    fallenAngel.scale.set(sceneWidth * 0.0002);
    startScene.addChild(fallenAngel);

    // Creating angel + forgiving hitbox
    angel = new Angel(sceneWidth / 2, sceneHeight - 150, angelFrames);
    angel.animationSpeed = 1/10; // Animation speed should match with the sound of wings
    angel.scale.set(sceneWidth * 0.0003);
    angel.y = sceneHeight - angel.height / 2;
    gameScene.addChild(angel);
    angel.play();

    // Load sounds
    wingSound = new Howl({
        src: ['sounds/wings-flapping.mp3', 'sounds/wings-flapping.wav'],
        loop: true,
    });

    shieldSound = new Howl({
        src: ['sounds/power-up.mp3', 'sounds/power-up.wav'],
        volume: 0.1
    });

    createLabelsAndButtons();

    // Start update loop
    app.ticker.add(gameLoop);
}

function createLabelsAndButtons() {
    
    // 1 - set up startscene
    let title = new PIXI.Text("FALLEN ANGEL");
    title.style = new PIXI.TextStyle({
        fill: 0xFFFFFF,
        fontSize: 72,
        fontFamily: 'Arial',
        fontStyle: 'bold'
        
    });
    title.x = sceneWidth / 2 - title.width / 2;
    title.y = 125;
    startScene.addChild(title);

    let quote = new PIXI.Text("Fly...fly as high as you can");
    quote.style = new PIXI.TextStyle({
        fill: 0xFFFFFF,
        fontSize: 30,
        fontFamily: 'Arial',
        fontStyle: 'italic'
    });
    quote.x = sceneWidth / 2 - quote.width / 2;
    quote.y = sceneHeight / 2 - 75;
    startScene.addChild(quote);

    let startLabel = new PIXI.Text("Press 'Space' to Fly");
    startLabel.style = new PIXI.TextStyle({
        fill: 0xFFFFFF,
        fontSize: 40,
        fontStyle: 'bold',
        fontFamily: 'Arial',
    });
    startLabel.x = sceneWidth / 2 - startLabel.width / 2;
    startLabel.y = sceneHeight / 2;
    startScene.addChild(startLabel);

    let instructions = new PIXI.Sprite.from("images/instructions.png");
    instructions.anchor.set(0.5, 0.5);
    instructions.scale.set(sceneWidth * 0.0003);
    instructions.x = sceneWidth / 2 - instructions.width / 2;
    instructions.y = sceneHeight / 2 + angel.height / 2;
    startScene.addChild(instructions);

    // 2 - set up 'gameScene'
    let textStyle = new PIXI.TextStyle({
        fill: 0xFFFFFF,
        fontSize: 30,
        fontFamily: 'Arial',
    });

    scoreLabel = new PIXI.Text();
    scoreLabel.style = textStyle;
    scoreLabel.x = 0;
    scoreLabel.y = 0;
    gameScene.addChild(scoreLabel);
    increaseScoreBy(0);

    attackBar = new PIXI.Graphics();
    attackBar.x = sceneWidth / 2 - 150;
    attackBar.beginFill(0xFFFFFF, 0.8);
    attackBar.drawRect(0, 0, 300, 30);
    attackBar.endFill();
    gameScene.addChild(attackBar);

    atkReadyLabel = new PIXI.Text();
    atkReadyLabel.style = new PIXI.TextStyle({
        fill: 0xFFFFFF,
        fontSize: 20,
        fontFamily: 'Arial',
        fontStyle: 'bold',
    });
    atkReadyLabel.x = attackBar.x;
    atkReadyLabel.y = attackBar.height + 5;
    gameScene.addChild(atkReadyLabel);

    // 3 - set up 'gameOverScene'
    gameOverScoreLabel = new PIXI.Text();
    gameOverScoreLabel.style = textStyle;
    gameOverScoreLabel.y = sceneHeight / 4;
    gameOverScene.addChild(gameOverScoreLabel);

    let buttonStyle = new PIXI.TextStyle({
        fill: 0xCCCCCC,
        fontSize: 25,
        fontFamily: 'Arial',
        fontStyle: 'bold'
    });

    retryButton = new PIXI.Text("Keep flying...?");
    retryButton.style = buttonStyle;
    retryButton.interactive = true;
    retryButton.buttonMode = true;
    retryButton.x = sceneWidth / 2 - retryButton.width / 2;
    retryButton.y = sceneHeight / 2 - retryButton.height / 2;
    retryButton.on("pointerup", startGame);
    retryButton.on("pointerover", e=>e.target.alpha = 0.7);
    retryButton.on("pointerout", e=>e.currentTarget.alpha = 1.0);
    gameOverScene.addChild(retryButton);

    returnHomeButton = new PIXI.Text("Return to Homepage");
    returnHomeButton.style = buttonStyle;
    returnHomeButton.interactive = true;
    returnHomeButton.buttonMode = true;
    returnHomeButton.x = sceneWidth / 2 - returnHomeButton.width / 2;
    returnHomeButton.y = sceneHeight / 2 + retryButton.height;
    returnHomeButton.on("pointerup", e=>window.location.href="index.html");
    returnHomeButton.on("pointerover", e=>e.target.alpha = 0.7);
    returnHomeButton.on("pointerout", e=>e.currentTarget.alpha = 1.0);
    gameOverScene.addChild(returnHomeButton);
}

function startGame() {
    startScene.visible = false;
    gameOverScene.visible = false;
    gameScene.visible = true;

    levelNum = 1;
    score = 0;
    meteorCount = 0;
    spawnCoolDown = 100;
    elapsedFrames = 0;
    angel.x = sceneWidth / 2;
    
    angel.hitArea.width = angel.width * 0.08;
    angel.hitArea.height = angel.height / 2.5;
    // Since there's no way to anchor the hitArea's origin point at its center,
    // it has to manually be repositioned
    angel.hitArea.x = angel.position.x - angel.hitArea.width / 2;
    angel.hitArea.y = angel.position.y - angel.hitArea.height / 2;

    attackBar.width = 0;
    atkDisabled = true;
    wingSound.play();
    loadLevel();
}

function gameLoop() {
    rechargeAttack();
    getUserInput();
    if (paused) return;

    // If angel is already invincible, prevent user from activating attack before next full recharge
    if(!angel.isVulnerable) {
        angel.tint = 0xFF0000; // maybe replace with animations if time allows it
        atkDisabled = true;

        // If attack bar isn't empty yet, stay invincible and continue reducing attack bar
        if (attackBar.width > 0) {
            attackBar.width -= 0.06 * app.ticker.elapsedMS;
        }
        else {
            // Otherwise, angel will become vulnerable once again
            angel.isVulnerable = true;
            angel.tint = 0xFFFFFF;
        }
    }
    // // Calculating delta time
    // let dt = 1/app.ticker.FPS;
    // if (dt > 1/12) dt = 1/12;
    
    sky.tilePosition.y += 1;
    
    // Keeping angel within screen
    let angel_x = angel.x;
    let hitbox_x = angel.hitArea.x;
    // let currentY = angel.y;
    angel.x = clamp(angel_x, 0 + angel.width / 2, sceneWidth - angel.width / 2);
    angel.hitArea.x = clamp(hitbox_x, 0 + angel.width / 2 - angel.hitArea.width / 2, sceneWidth - angel.width / 2 - angel.hitArea.width / 2);
    // Since vertical movement is not yet implemented, this isn't needed
    // angel.y = clamp(currentY, 0 + angel.height / 2, sceneHeight - angel.height);
    
    // Spawn meteor if enough time has passed
    if(elapsedFrames > spawnCoolDown) {
        spawnMeteor();
        // Reset frame counter
        elapsedFrames = 0;
    }
    else {
        elapsedFrames += app.ticker.deltaTime;
    }
    
    // Move meteors
    for (let m of meteors) {
        m.move();
    }
    
    // If all meteors for the level have fallen, level up
    if (meteorCount >= 5 * levelNum) {
        levelNum++;
        loadLevel();
    }

    // Collision detection
    for(let m of meteors) {
        if(m.isAlive && rectsIntersect(m, angel.hitArea)) {
            if (angel.isVulnerable) {
                end();
                return;
            }
            else {
                gameScene.removeChild(m);
                m.isAlive = false;
            }
        }
    }

    // Cleaning up
    meteors = meteors.filter(m => m.isAlive);

    increaseScoreBy(angel.speed);
}

function loadLevel() {
    // For each level...
    // Max meteors to pass through increases by 10 (see level up)
    // Counter resets
    meteorCount = 0;
    if(spawnCoolDown > minCoolDown) {
        spawnCoolDown = (spawnCoolDown * 0.8).toFixed(2); // cool-down time is reduced by 20%
    }
    let newSpeed = angel.speed * Math.pow(1.2, levelNum - 1);
    if (newSpeed < angel.maxSpeed) angel.speed = newSpeed; // angel speed increases (score increases faster)
    paused = false;
}

function increaseScoreBy(value) {
    score += value;
    scoreLabel.text = `Height: ${score.toFixed(2)}m`;
}

function rechargeAttack() {
    // Recharges attack bar by increasing its width by 20 pixels per second
    if(attackBar.width < attackBarMax) {
        atkReadyLabel.alpha = 0; // invisible if the attack is not ready
        attackBar.width = attackBar.width + (0.02 * app.ticker.elapsedMS);
    }
    else {
        atkReadyLabel.text = "Press 'Space' to activate shield!";
        atkReadyLabel.alpha = 1;
        atkDisabled = false;
    }
}

function activateInvincibility() {
    // Sets angel to invincible
    angel.isVulnerable = false;
    shieldSound.play();
}

function spawnMeteor() {
    meteorCount++;
    let m = new Meteor(sceneHeight * 0.1, 0, 0);
    let offsetX = Math.floor(m.width / 2);
    let offsetY = Math.floor(m.height / 2);
    let randomX = getRandom(0 + offsetX, sceneWidth - offsetX);
    m.x = randomX;
    m.y = -offsetY;
    let newSpeed = m.speed + (levelNum - 1);
    if(newSpeed < m.maxSpeed) {
        m.speed = newSpeed;
    }
    // Meteors always spawn from the top
    meteors.push(m);
    gameScene.addChild(m);
}

function end() {
    paused = true;
    // clear out level
    meteors.forEach(m => gameScene.removeChild(m));
    meteors = [];
    wingSound.stop();

    gameOverScene.visible = true;
    gameScene.visible = false;

    gameOverScoreLabel.text = `You flew ${score.toFixed(2)}m high.`;
    gameOverScoreLabel.x = sceneWidth / 2 - gameOverScoreLabel.width / 2;
}

function getUserInput() {
    // Shift = 16
    // Holding shift reduces movement speed for better accuracy
    let distance = 10;
    if(keyState[16]) {
        distance = 5;
    }
    // Left arrow = 37
    // A = 65
    if (keyState[37] || keyState[65]){
        angel.position.x -= distance;
        angel.hitArea.x -= distance;
    }    
    // Right = 39
    // D = 68
    if (keyState[39] || keyState[68]){
        angel.position.x += distance;
        angel.hitArea.x += distance;
    }
    // Space key = 32
    if (keyState[32]) {
        // Start game with space bar
        if (startScene.visible){
            startGame();
        }
        // Or activate invincibility (shows error message if attack bar is not full)
        else if (gameScene.visible) {
            if (atkDisabled) {
                atkReadyLabel.text = "Shield needs to be recharged!";
                atkReadyLabel.alpha = 0.8;
            }
            else {
                activateInvincibility();
            }
        }
    }
}

// #region Function graveyard
// // https://codepen.io/SkuliOskarsson/pen/ZbJKVW
// function onKeyDown(key) {
//     // A Key is 65
//     // Left arrow is 37
//     if (key.keyCode == 65 || key.keyCode == 37) {
//         if (angel.position.x != 0) {
//             angel.position.x -= 10;
//         }
//     }

//     // D Key is 68
//     // Right arrow is 39
//     if (key.keyCode == 68 || key.keyCode == 39) {
//         if (angel.position.x != sceneWidth) {
//             angel.position.x += 10;
//         }
//     }

//     // Space key is 32
//     if (key.keyCode == 32 && startScene.visible) {
//         startGame();
//     }
// }
// #endregion