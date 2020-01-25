class Angel extends PIXI.extras.AnimatedSprite {
    constructor(x = 0, y = 0, textures) {
        super(textures);
        this.anchor.set(.5, .5);
        this.x = x;
        this.y = y;

        // variables
        this.isVulnerable = true;
        this.speed = 0.1;
        this.maxSpeed = 1;
        this.hitArea = new PIXI.Rectangle(0, 0, 0, 0);
    }
}

// Need to change sprite to meteor
class Meteor extends PIXI.Sprite {
    constructor(radius, x = 0, y = 0) {
        super(PIXI.loader.resources["images/meteor.png"].texture);
        this.anchor.set(.5, .5);
        this.x = x;
        this.y = y;
        
        // variables
        // this.fwd = {0:0, 1:1};
        this.radius = radius;
        this.speed = 10;
        this.maxSpeed = 25; 
        this.isAlive = true;
        Object.seal(this);
    }

    move() {
        this.y += this.speed;
    }
}