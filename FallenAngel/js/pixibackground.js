/*
*  PixiJS Background Cover/Contain Script
*   Returns object
* . {
*       container: PixiJS Container
* .     doResize: Resize callback
*   } 
*   ARGS:
*   bgSize: Object with x and y representing the width and height of background. Example: {x:1280,y:720}
*   inputSprite: Pixi Sprite containing a loaded image or other asset.  Make sure you preload assets into this sprite.
*   type: String, either "cover" or "contain".
*   forceSize: Optional object containing the width and height of the source sprite, example:  {x:1280,y:720}
*/
function background(bgSize, inputSprite, type, forceSize) {
    var sprite = inputSprite;
    var bgContainer = new PIXI.Container();
    var mask = new PIXI.Graphics().beginFill(0x8bc5ff).drawRect(0,0, bgSize.x, bgSize.y).endFill();
    bgContainer.mask = mask;
    bgContainer.addChild(mask);
    bgContainer.addChild(sprite);
    
    function resize() {
        var sp = {x:sprite.width,y:sprite.height};
        if(forceSize) sp = forceSize;
        var winratio = bgSize.x/bgSize.y;
        var spratio = sp.x/sp.y;
        var scale = 1;
        var pos = new PIXI.Point(0,0);
        if(type == 'cover' ? (winratio > spratio) : (winratio < spratio)) {
            //photo is wider than background
            scale = bgSize.x/sp.x;
            pos.y = -((sp.y*scale)-bgSize.y)/2
        } else {
            //photo is taller than background
            scale = bgSize.y/sp.y;
            pos.x = -((sp.x*scale)-bgSize.x)/2
        }

        sprite.scale = new PIXI.Point(scale,scale);
        sprite.position = pos;
    }
    
    resize();

    return {
        container: bgContainer,
        doResize: resize
    }
}