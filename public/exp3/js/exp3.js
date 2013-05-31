function Exp3( ctx )
{
    var thiz = this;
    thiz.context = ctx;
    
    function init()
    {
        // basic css setup
        document.body.style["margin"] = "0px";
        document.body.style["background-color"] = thiz.context.bgcolor ? thiz.context.bgcolor : "#000000";
        document.body.style["overflow"] = "hidden";

        // WebGLRenderer
        thiz.renderer = new THREE.WebGLRenderer();
        thiz.renderer.setSize( window.innerWidth, window.innerHeight );
        document.body.appendChild( thiz.renderer.domElement );

        // defaults
        thiz.defaults = {};
        
        thiz.defaults.camera = new THREE.PerspectiveCamera( 70, window.innerWidth / window.innerHeight, 1, 1000 );
        
        thiz.defaults.scene = new THREE.Scene();

        window.addEventListener( 'resize', onWindowResize, false );
        
        if ( thiz.context.on_init )
            thiz.context.on_init.call( thiz );
    }

    function onWindowResize()
    {
        if ( thiz.context.on_resize )
            thiz.context.on_resize.call( thiz, window.innerWidth, window.innerHeight );
        
        thiz.renderer.setSize( window.innerWidth, window.innerHeight );
    }

    function animate()
    {
        requestAnimationFrame( animate );

        if ( thiz.context.on_render )
            thiz.context.on_render.call( thiz );
    }
    
    init();
    animate();
}

