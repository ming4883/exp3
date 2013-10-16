function Exp3( ctx )
{
    var thiz = this;
    thiz.context = ctx;
    thiz.context.bgcolor = thiz.context.bgcolor ? thiz.context.bgcolor : "#000000";
    thiz.context.bgcolorObj = new THREE.Color( thiz.context.bgcolor );
    
    function init()
    {
        // basic css setup
        document.body.style["margin"] = "0px";
        document.body.style["background-color"] = thiz.context.bgcolor;
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
    
    thiz.enableStats = function ()
    {
        this.stats = new Stats();
        this.stats.setMode( 0 );
        
        this.stats.domElement.style.position = "absolute";
        this.stats.domElement.style.left = "0px";
        this.stats.domElement.style.bottom = "0px";
        document.body.appendChild( this.stats.domElement );
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
        
        if ( thiz.stats )
            thiz.stats.begin();

        if ( thiz.context.on_render )
            thiz.context.on_render.call( thiz );
            
        if ( thiz.stats )
            thiz.stats.end();
    }
    
    init();
    animate();
}

Exp3.prototype._shaderId = 0;

Exp3.prototype.createPFxShaderPass = function( material )
{
    var ret = {};
    ret.material = material;
    
    ret.render = function( composer )
    {
        composer.quad.material = this.material;
        composer._renderer.render( composer.scene, composer.camera );
    }
    
    return ret;
}

Exp3.prototype.createPFxRenderPass = function( scene, camera )
{
    var ret = {};
    ret.scene = scene;
    ret.camera = camera;
    
    ret.render = function( composer )
    {
        composer._renderer.render( this.scene, this.camera );
    }
    
    return ret;
}

Exp3.prototype.createPFxComposer = function()
{
    var ret = {};
    ret.camera = new THREE.PerspectiveCamera( 70, window.innerWidth / window.innerHeight, 1, 1000 );
        
    ret._renderer = this.renderer;
    
    ret.quadGeom = this.createScreenQuad();
    ret.quad = new THREE.Mesh( ret.quadGeom, null );
    
    ret.scene = new THREE.Scene();
    ret.scene.add( ret.quad );
    
    ret.passes = [];
    
    ret.render = function()
    {
        for ( var p = 0; p < this.passes.length; ++p )
            this.passes[p].render( this );
    };
    
    return ret;
}

Exp3.prototype.createScreenQuad = function()
{
    var geom = new THREE.Geometry();
    geom.vertices.push( new THREE.Vector3( -1, 1, 0 ) );
    geom.vertices.push( new THREE.Vector3( -1,-1, 0 ) );
    geom.vertices.push( new THREE.Vector3(  1, 1, 0 ) );
    geom.vertices.push( new THREE.Vector3(  1,-1, 0 ) );
    
    var uv = [
        new THREE.Vector2( 0, 1 ),
        new THREE.Vector2( 0, 0 ),
        new THREE.Vector2( 1, 1 ),
        new THREE.Vector2( 1, 0 ),
        ];
    
    geom.faceVertexUvs[0].push( [ uv[0].clone(), uv[1].clone(), uv[2].clone() ] );
    geom.faceVertexUvs[0].push( [ uv[3].clone(), uv[2].clone(), uv[1].clone() ] );
    
    geom.faces.push( new THREE.Face3( 0, 1, 2 ) );
    geom.faces.push( new THREE.Face3( 3, 2, 1 ) );
    
    return geom;
}

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

/*
THREE.Exp3CubeGeometry = function ( width, height, depth, widthSegments, heightSegments, depthSegments ) {

    THREE.Geometry.call( this );

    var scope = this;

    this.width = width;
    this.height = height;
    this.depth = depth;

    this.widthSegments = widthSegments || 1;
    this.heightSegments = heightSegments || 1;
    this.depthSegments = depthSegments || 1;

    var width_half = this.width / 2;
    var height_half = this.height / 2;
    var depth_half = this.depth / 2;

    buildPlane( 'z', 'y', - 1, - 1, this.depth, this.height, width_half, 0 ); // px
    buildPlane( 'z', 'y',   1, - 1, this.depth, this.height, - width_half, 1 ); // nx
    buildPlane( 'x', 'z',   1,   1, this.width, this.depth, height_half, 2 ); // py
    buildPlane( 'x', 'z',   1, - 1, this.width, this.depth, - height_half, 3 ); // ny
    buildPlane( 'x', 'y',   1, - 1, this.width, this.height, depth_half, 4 ); // pz
    buildPlane( 'x', 'y', - 1, - 1, this.width, this.height, - depth_half, 5 ); // nz

    function buildPlane( u, v, udir, vdir, width, height, depth, materialIndex ) {

        var w, ix, iy,
        gridX = scope.widthSegments,
        gridY = scope.heightSegments,
        width_half = width / 2,
        height_half = height / 2,
        offset = scope.vertices.length;
        var flipS = false;
        var flipT = false;

        if ( ( u === 'x' && v === 'y' ) || ( u === 'y' && v === 'x' ) ) {

            w = 'z';

        } else if ( ( u === 'x' && v === 'z' ) || ( u === 'z' && v === 'x' ) ) {

            w = 'y';
            gridY = scope.depthSegments;
            //flipT = true;

        } else if ( ( u === 'z' && v === 'y' ) || ( u === 'y' && v === 'z' ) ) {

            w = 'x';
            gridX = scope.depthSegments;
            flipS = true;

        }

        var gridX1 = gridX + 1,
        gridY1 = gridY + 1,
        segment_width = width / gridX,
        segment_height = height / gridY,
        normal = new THREE.Vector3();

        normal[ w ] = depth > 0 ? 1 : - 1;

        for ( iy = 0; iy < gridY1; iy ++ ) {

            for ( ix = 0; ix < gridX1; ix ++ ) {

                var vector = new THREE.Vector3();
                vector[ u ] = ( ix * segment_width - width_half ) * udir;
                vector[ v ] = ( iy * segment_height - height_half ) * vdir;
                vector[ w ] = depth;

                scope.vertices.push( vector );

            }

        }

        for ( iy = 0; iy < gridY; iy++ ) {

            for ( ix = 0; ix < gridX; ix++ ) {

                var a = ix + gridX1 * iy;
                var b = ix + gridX1 * ( iy + 1 );
                var c = ( ix + 1 ) + gridX1 * ( iy + 1 );
                var d = ( ix + 1 ) + gridX1 * iy;

                var face = new THREE.Face4( a + offset, b + offset, c + offset, d + offset );
                face.normal.copy( normal );
                face.vertexNormals.push( normal.clone(), normal.clone(), normal.clone(), normal.clone() );
                face.materialIndex = materialIndex;
                
                var s0 = ix / gridX;
                var s1 = ( ix + 1 ) / gridX;
                
                var t0 = 1 - iy / gridY;
                var t1 = 1 - ( iy + 1 ) / gridY;
                
                if ( flipS ) {
                    s0 = 1 - s0;
                    s1 = 1 - s1;
                }
                
                if ( flipT ) {
                    t0 = 1 - t0;
                    t1 = 1 - t1;
                }

                scope.faces.push( face );
                scope.faceVertexUvs[ 0 ].push( [
                            new THREE.Vector2( s0, t0 ),
                            new THREE.Vector2( s0, t1 ),
                            new THREE.Vector2( s1, t1 ),
                            new THREE.Vector2( s1, t0 )
                        ] );

            }

        }

    }

    this.computeCentroids();
    this.mergeVertices();

};

THREE.Exp3CubeGeometry.prototype = Object.create( THREE.Geometry.prototype );
*/
