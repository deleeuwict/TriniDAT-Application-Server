	/* 
		TriniDAT App Store Frontend.
		Copyright (c) 2013 GertJan de Leeuw | De Leeuw ICT | http://www.deleeuwict.nl
		
		Code quality or be a stay-at-home mom. The world is tired of clicking on garbage! industry++.
		
		
		Acknowledgement:
		
		- JIT by Nicolas Garcia Belmonte - http://philogb.github.io/jit/.
	*/
	
	
	function getClean(str)
	{
		//remove non-decoded strings.
		return str.replace(/\+/g, str);
	}
	
	function JSONize_TriniDAT_app(TriniDAT_app)
	{
		var dataset_JSONize;
		var tdat_app;
		var tdat_app_data;
		var tdatapp_mapping_point;
		var tdatapp_mapping_point_data = new Object();
		var appid = TriniDAT_app["@id"];
		
		//alert(TriniDAT_app["@author"]);

		tdat_app_data = new Object();
		tdat_app_data.$area = 270;
		tdat_app_data.url = "/"  + TriniDAT_app["@id"];
		tdat_app_data.fullurl = TriniDAT_app["@fullurl"];
		tdat_app_data.ntype = "app";
		
		//initialize application
		tdat_app = new Object();
		tdat_app.data = tdat_app_data;
		tdat_app.id = appid;
		tdat_app.name = getClean(unescape(TriniDAT_app["@name"])) + " id " + tdat_app.id;
		tdat_app.children = new Array();		
		
			if ( !TriniDAT_app.mps.mp.length)
			{
				var single_mapping_point = TriniDAT_app.mps.mp;
				TriniDAT_app.mps.mp = new Array(single_mapping_point);			
			}
		
		for (var x=0;x<TriniDAT_app.mps.mp.length;x++)
		{
			var mp_id = x+1;
			var mp_item = TriniDAT_app.mps.mp[x];
			
			tdatapp_mapping_point_data = new Object();
			
			//Node tag.
			tdatapp_mapping_point_data.$area = 270;
			tdatapp_mapping_point_data.url = mp_item["@url"];
			tdatapp_mapping_point_data.appname = tdat_app.name; 
			tdatapp_mapping_point_data.fullurl = mp_item["@fullurl"];
			tdatapp_mapping_point_data.ntype = "mp";
			
			//Mapping Point item.
			tdatapp_mapping_point = new Object();
			tdatapp_mapping_point.children = new Array();
			
				if ( mp_item.dependencies )
				{				
						if ( !mp_item.dependencies.dependency.length)
						{
							//Make array.
							var single_dependency = mp_item.dependencies.dependency;
							mp_item.dependencies.dependency = new Array(single_dependency);
						}
					
					for ( var y=0;y < mp_item.dependencies.dependency.length;y++ )
					{

						if ( mp_item.dependencies.dependency[y]["@name"] )
						{
							if ( mp_item.dependencies.dependency[y]["@name"].length > 1  )
							{
						
								var dep_item = new Object();
								dep_item.children = new Array();
								
								dep_item.data = tdatapp_mapping_point_data;
								dep_item.name = mp_item.dependencies.dependency[y]["@name"];
								dep_item.id =  "tdat_app_" + appid + "_mp_" + mp_id + "_dep" + y;
								dep_item.ntype = "mod";
							
							}

							tdatapp_mapping_point.children.push(dep_item);
						}
					}
				}
			
			
			tdatapp_mapping_point.data = tdatapp_mapping_point_data;
			tdatapp_mapping_point.id = "tdat_app_" + appid + "_mp_" + mp_id;
			tdatapp_mapping_point.name = mp_item["@url"];
	

			
			//push on mp list.
			tdat_app.children.push(tdatapp_mapping_point);	
		}
		

		return tdat_app;
	}

	
	function parse_TriniDAT_xml(apps_xml)
	{
		var TriniDAT_AppList = new Array();
		var app_cache = eval('(' + unescape(apps_xml) + ')');  
		
		//debug.
		//document.write(unescape(apps_xml));
		
			if ( !app_cache.length)
			{
				var single_app= app_cache.app;
				app_cache = new Array(single_app);
				
			}		
		
		for (var x=0;x<app_cache.length;x++)
		{
			var app_object;
			
			app_object = JSONize_TriniDAT_app(app_cache[x].app);
			TriniDAT_AppList.push(app_object);
		}
	
		return TriniDAT_AppList;
	}
	

	var root_node = new Object();
	
	root_node.children = parse_TriniDAT_xml(master_list);
	root_node.id = "0";
	root_node.name = "TriniDAT Application List";
	
	//reset global var.
	dataset_json = JSON.stringify(root_node);


var labelType, useGradients, nativeTextSupport, animate;

(function() {
  var ua = navigator.userAgent,
      iStuff = ua.match(/iPhone/i) || ua.match(/iPad/i),
      typeOfCanvas = typeof HTMLCanvasElement,
      nativeCanvasSupport = (typeOfCanvas == 'object' || typeOfCanvas == 'function'),
      textSupport = nativeCanvasSupport 
        && (typeof document.createElement('canvas').getContext('2d').fillText == 'function');
  //I'm setting this based on the fact that ExCanvas provides text support for IE
  //and that as of today iPhone/iPad current text support is lame
  labelType = (!nativeCanvasSupport || (textSupport && !iStuff))? 'Native' : 'HTML';
  nativeTextSupport = labelType == 'Native';
  useGradients = nativeCanvasSupport;
  animate = !(iStuff || !nativeCanvasSupport);
})();

var Log = {
  elem: false,
  write: function(text){
    if (!this.elem) 
      this.elem = document.getElementById('log');
    this.elem.innerHTML = text;
    this.elem.style.left = (500 - this.elem.offsetWidth / 2) + 'px';
  }
};


function init(){
  //init data
  var json = dataset_json;
  
  //end
  //init TreeMap
  var tm = new $jit.TM.Squarified({
    //where to inject the visualization
    injectInto: 'infovis',
    //show only one tree level
    levelsToShow: 1,
    //parent box title heights
    titleHeight: 0,
    //enable animations
    animate: animate,
    //box offsets
    offset: 1,
    //use canvas text
    Label: {
      type: labelType,
      size: 24,
	   color: 'white',
      family: 'BankGothic Md BT'
    },
    //enable specific canvas styles
    //when rendering nodes
    Node: {
      CanvasStyles: {
        shadowBlur: 0,
        shadowColor: '#000'
      }
    },
    //Attach left and right click events
    Events: {
      enable: true,
      onClick: function(node) {
        if(node) 
			{
				var data = node.data;
			//	alert(data.url);
				tm.enter(node);
				 
			}
      },
      onRightClick: function() {
        tm.out();
      },
      //change node styles and canvas styles
      //when hovering a node
      onMouseEnter: function(node, eventInfo) {
        if(node) {
          //add node selected styles and replot node
          node.setCanvasStyle('shadowBlur', 7);
          node.setData('color', '#2F4F4F');
          tm.fx.plotNode(node, tm.canvas);
          tm.labels.plotLabel(tm.canvas, node);
        }
      },
      onMouseLeave: function(node) {
        if(node) {
          node.removeData('color');
          node.removeCanvasStyle('shadowBlur');
          tm.plot();
        }
      }
    },
    //duration of the animations
    duration: 300,
    //Enable tips
    Tips: {
      enable: true,
      type: 'Native',
      //add positioning offsets
      offsetX: 20,
      offsetY: 20,
      //implement the onShow method to
      //add content to the tooltip when a node
      //is hovered
      onShow: function(tip, node, isLeaf, domElement) {
        var html = "<div class=\"tip-title\">" + node.name 
          + "</div><div class=\"tip-text\">";
        var data = node.data;
		
		
		
			if ( data)
			{
				if ( data.ntype)
				{
						if ( data.ntype == "app" )
						{

							if(data.fullurl) {
							  html += "App: " + data.url + "<br />URI: " +  data.fullurl  + " <br/>";
							}
							if(data.playcount) {
							  html += "Runtimes: " + data.runtimecount;
							}
							if(data.image) {
							  html += "<img src=\""+ data.image +"\" class=\"album\" />";
							}
							tip.innerHTML =  html; 						
						
						}
						else if( data.ntype == "mp" )
						{
							if(data.fullurl) {
							  html += "Owner: &#39;" +  data.appname + "&#39;<br />URL: " +  data.fullurl  + " <br/>";
							}							
						}	
						else if( data.ntype == "mod" )
						{
							
						}
						
					tip.innerHTML =  html; 								
				}
			}
			

      }  
    },
    //Implement this method for retrieving a requested  
    //subtree that has as root a node with id = nodeId,  
    //and level as depth. This method could also make a server-side  
    //call for the requested subtree. When completed, the onComplete   
    //callback method should be called.  
    request: function(nodeId, level, onComplete){  
      var tree = eval('(' + json + ')');  
      var subtree = $jit.json.getSubtree(tree, nodeId);  
      $jit.json.prune(subtree, 1);  
      onComplete.onComplete(nodeId, subtree);  
    },
    //Add the name of the node in the corresponding label
    //This method is called once, on label creation and only for DOM labels.
    onCreateLabel: function(domElement, node){
        domElement.innerHTML = node.name;
    }
  });
  
  var pjson = eval('(' + json + ')');  
  $jit.json.prune(pjson, 1);
  tm.loadJSON(pjson);
  tm.refresh();
  //end
  var sq = $jit.id('r-sq'),
      st = $jit.id('r-st'),
      sd = $jit.id('r-sd');
  var util = $jit.util;
  util.addEvent(sq, 'change', function() {
    if(!sq.checked) return;
    util.extend(tm, new $jit.Layouts.TM.Squarified);
    tm.refresh();
  });
  util.addEvent(st, 'change', function() {
    if(!st.checked) return;
    util.extend(tm, new $jit.Layouts.TM.Strip);
    tm.layout.orientation = "v";
    tm.refresh();
  });
  util.addEvent(sd, 'change', function() {
    if(!sd.checked) return;
    util.extend(tm, new $jit.Layouts.TM.SliceAndDice);
    tm.layout.orientation = "v";
    tm.refresh();
  });
  //add event to the back button
  var back = $jit.id('back');
  $jit.util.addEvent(back, 'click', function() {
    tm.out();
  });
}
