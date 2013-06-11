function Exp3( ctx )
{
    var thiz = this;
    thiz.context = ctx;
    
    function init()
    {
        // basic css setup
        document.body.style["margin"] = "0px";
        document.body.style["background-color"] = thiz.context.bgcolor ? thiz.context.bgcolor : "#000000";
        document.body.style["color"] = thiz.context.fgcolor ? thiz.context.fgcolor : "#ffffff";
        document.body.style["overflow"] = "hidden";
        
        var elems;
        // exp3.info
        elems = document.getElementsByClassName( "exp3.info" );
        for ( var i = 0; i < elems.length; ++i )
        {
            var elem = elems[i];
            elem.style["padding"] = "5px";
            elem.style["font-family"] = "Monospace";
            elem.style["font-size"] = "12px";
            elem.style["position"] = "absolute";
            elem.style["text-shadow"] = "2px 2px 2px #404040";
            elem.style["z-index"] = "1000";
        }
        
        // exp3.link
        elems = document.getElementsByClassName( "exp3.link" );
        for ( var i = 0; i < elems.length; ++i )
        {
            var elem = elems[i];
            elem.style["color"] = "orange";
        }
        
        document.body.style["-webkit-touch-callout"] = "none";
        document.body.style["-webkit-user-select"] = "none";
        document.body.style["-khtml-user-select"] = "none";
        document.body.style["-moz-user-select"] = "none";
        document.body.style["-ms-user-select"] = "none";
        document.body.style["user-select"] = "none";
        
        //document.body.onselectstart = function() { return false; }
        
        // WebGLRenderer
        thiz.renderer = new THREE.WebGLRenderer();
        thiz.renderer.setSize( window.innerWidth, window.innerHeight );
        document.body.appendChild( thiz.renderer.domElement );
        
        // Clock
        thiz.clock = new THREE.Clock();
        // thiz.clock.getDelta();
        // thiz.clock.getElapsedTime();

        // defaults
        thiz.defaults = {};
        
        thiz.defaults.camera = new THREE.PerspectiveCamera( 70, window.innerWidth / window.innerHeight, 1, 1000 );
        
        thiz.defaults.scene = new THREE.Scene();

        window.addEventListener( 'resize', onWindowResize, false );
        
        if ( thiz.context.on_init )
            thiz.context.on_init.call( thiz );
        
        // disable right-click context menu
        thiz.renderer.domElement.addEventListener( "contextmenu", function( evt )
        { 
            if ( evt.button === 2 )
            {
                evt.preventDefault();
                return false;
            }
        }, false );
        
        // mouse events
        if ( thiz.context.on_mousedown )
        {
            thiz.renderer.domElement.onmousedown = function( evt )
            {
                thiz.context.on_mousedown.call( thiz, evt );
                
                evt.preventDefault();
                evt.stopPropagation();
            }
        }
        
        if ( thiz.context.on_mouseup )
        {
            thiz.renderer.domElement.onmouseup = function( evt )
            {
                thiz.context.on_mouseup.call( thiz, evt );
                
                evt.preventDefault();
                evt.stopPropagation();
            }
        }
        
        if ( thiz.context.on_mousemove )
        {
            thiz.renderer.domElement.onmousemove = function( evt )
            {
                thiz.context.on_mousemove.call( thiz, evt );
                
                evt.preventDefault();
                evt.stopPropagation();
            }
        }
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

Exp3.prototype._shaderId = 0;

Exp3.prototype.loadText = function( url )
{
    var request = new XMLHttpRequest();
    
    request.open( "GET", url, false );
    
    request.send( null ); 
    
    if ( request.status == 200 )
    {   // If we got HTTP status 200 (OK)
        //console.log( request.responseText );
        return request.responseText;
    }
    else
    {   // Failed
        console.log( "Exp3.loadText( " + url + " ) failed" );
        return "";
    }
}

Exp3.prototype.loadShaderMaterial = function( params )
{
    var vsScript = this.loadText( params.vertexShader );
    var fsScript = this.loadText( params.fragmentShader );
    
    return new THREE.ShaderMaterial( {
        uniforms: params.uniforms,
        vertexShader: vsScript,
        fragmentShader: fsScript,
    } );
}

