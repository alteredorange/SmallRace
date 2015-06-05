// FollowPath .... Mich Shire Feb. 2015

// Make the script also execute in edit mode.
@script ExecuteInEditMode()

var PathFolder : GameObject;
@Range (1.0, 20.0) 
var speed : float = 3.0;
var closedLoop : boolean;
var usePingpong : boolean;
var showPaths : boolean = true;
@Range (10, 100) 
var SlerpSegments : int = 20;
var useSlerp : boolean;
@Range (.01, .4) 
var slerpPoint: float = .1; // amount before turning
//enum SlerpColor { Black=0,White=1,Grey=2,Blue=3,Cyan=4,Green=5,Magenta=6,Red=7,Yellow=8}
//var slerpColor = SlerpColor.Cyan;
//var colorArray = Array();
//var scolor;

private var numpoints : int=0;
private var curindex : int ;  // current index
private var once : int = 0;
private var purpleArray = Array() ;
private var ppoint : float = 1.0 ;  // purple point
private var ptimes : int = 0 ;  // purple

private var waypoints = Array();
private var target : Vector3;
private var dir: Vector3; // direction
private var disttotarget: float;  //distance to target
private var distofline: float;  //distance of current line 
private var distnormal: float;  //distance normalized 
private var rot: Quaternion; // rotation 
private var turnam: float; // rotation 


function Update(){
		if (!Application.isPlaying) {
		RebuildWaypointList ();	DrawWaypoints (); return;
	}
	if(showPaths) DrawWaypoints (); // remark this out later to speed up the program
	if(once==0) RebuildWaypointList ();
		MoveForward();
	turnam=0.01;  // for no slerping = almost at zero
	if (useSlerp)turnam=slerpPoint; 
	if (distnormal<turnam)NextWayPoint();
}


//============= subroutines =============

function MoveForward(){
		FindDistance();	dir= target - transform.position;
	//==========================
	if(useSlerp){
		var rot: Quaternion = Quaternion.LookRotation(dir);
	transform.rotation = Quaternion.Slerp(transform.rotation, rot, speed * Time.deltaTime);
	} else {
		transform.LookAt(target);  // no Slerping
	}
	// move in the current forward direction at specified speed:
	// to keep it grounded , use Rigidbody and check "Use Gravity".
	transform.Translate(Vector3(0, 0, speed * Time.deltaTime));
}


function NextWayPoint(){
	if((curindex+1) < numpoints){  	// normal going forward
		curindex+=1;
	}else{
		curindex=0; 
	}
	// ========= if ping pong then reverse  =========
	if(usePingpong && curindex==0){  // then at the end	
		
			// swap allthe Vector3 info in waypoints()
		var n=numpoints-1; var nn=(numpoints)/2;
		for(var a : int = 0; a <nn; a++){
			var tempVector=waypoints[a];
			waypoints[a] = waypoints[n-a]; 
			waypoints[n-a] = tempVector; 
		}
		curindex=0; 	target=waypoints[curindex]; return;
	}
	// ========= if no loop then jump to beginning =========
	if(!closedLoop && curindex==0){
		curindex=1; target=waypoints[1];  
			// go to first waypoint and exit subroutine
		transform.position=waypoints[0]; 
		transform.LookAt(target); 
		return;
	}
	target=waypoints[curindex];   // else update target
}


function FindDistance(){
	var tempindex = curindex;
	if((tempindex-1) >-0.5){
		tempindex-=1;
	}else{
		tempindex=numpoints-1; 
	}
	distofline = Vector3.Distance(target,waypoints[tempindex]);
	disttotarget= Vector3.Distance(target,transform.position);
	distnormal= disttotarget/distofline;  // make a percentage  0.0 to 1.0
}


// =========================================
function RebuildWaypointList () {
	var allpoints = PathFolder.GetComponentsInChildren(Transform);
    for(var i : int = 1; i < allpoints.Length; i++){
		waypoints[i-1] = (allpoints[i].position); 
	}
	numpoints=allpoints.Length-1;
	once=1;   // flag to do this coroutine only once
	target=waypoints[curindex];  // starts at 0
}


// =========================================
function DrawWaypoints () {
	//if (Application.isPlaying)  return;
	for ( i=0; i < numpoints; i++){
		if (i<numpoints-1){  // open path  -1
			if (Physics.Linecast(waypoints[i],waypoints[i+1])) {  // true if collider
			Debug.DrawLine (waypoints[i],waypoints[i+1], Color.red);
			}else{
			Debug.DrawLine (waypoints[i],waypoints[i+1], Color.green);
			}
		}
		else if(closedLoop)	{  // closed loop
			if (Physics.Linecast(waypoints[i],waypoints[0])) {  // true if collider
			Debug.DrawLine (waypoints[i],waypoints[0], Color.red);
			}else{
			Debug.DrawLine (waypoints[i],waypoints[0], Color.blue);
			}
		}
	}
	
	if (Application.isPlaying) {  // game is running ... draw purple path
		ppoint -=Time.deltaTime;   // one second?

		if(ppoint<0){
					//print("ptimes1 =" + ptimes + " : purpArray ="+purpleArray);
			purpleArray.Add(this.transform.position); ptimes +=1;
			if(ptimes>SlerpSegments){
				purpleArray.RemoveAt(0);  // remove the first index
				ptimes -=1; 
			}
			//print("ptimes2 =" + ptimes + " : purpArray ="+purpleArray);
			ppoint=(1/speed)*.75;
		}
		
		SelectColor();
		
		if(ptimes<1 || !useSlerp)return;
		for ( i=0; i < ptimes-1; i++){
			//Debug.Log("parray" +i+"=" +purpleArray[i]+ " parray"+(i+1)+"=" + purpleArray[i+1]);
			var a = purpleArray[i]; 
			var b = purpleArray[i+1];
			//Debug.Log(">>" + a + b);
			Debug.DrawLine (a,b, Color.magenta);
		}
	}
}

function SelectColor(){
// I gave up on this after a whole day...... maybe you can get it working.
	
//var colors : Color[]={Color.black=0,Color.white=1}
//,Color.grey,Color.blue,Color.cyan,Color.green,Color.magenta,Color.red,Color.yellow}
//GUI.color = new Color( 1, 1, 1, 0.5f )

	//GUI.color = colors[1];
	//Black=0,White=1,Grey=2,Blue=3,Cyan=4,
	//Green=5,Magenta=6,Red=7,Yellow=8
	
	//print("GUIcolor="+GUI.color);
	
//int indx= Random.Range(0,colors.Length);

//GUIText.material.color = colors[indx];
	
		//	print("color="+slerpColor);
	//if(slerpColor="Magenta")scolor="magenta";
	//colorArray[0] = Vector4(0,0,0,1);
	//colorArray[1] = Vector4(1,1,1,1);
	//var cVector = colorArray[1];  	print ("cv " +cVector);
	//GUI.color = new Color(colorArray[1]);

	//print (colorArray);
	//var color : Color = Color(0.2, 0.3, 0.4, 0.5);
	//print ("v=" + colorArray[1]);
		//GUI.color= colorArray[1];
	
	/*
	black	Solid black. RGBA is (0, 0, 0, 1).
blue	Solid blue. RGBA is (0, 0, 1, 1).
clear	Completely transparent. RGBA is (0, 0, 0, 0).
cyan	Cyan. RGBA is (0, 1, 1, 1).
gray	Gray. RGBA is (0.5, 0.5, 0.5, 1).
green	Solid green. RGBA is (0, 1, 0, 1).
grey	English spelling for gray. RGBA is the same (0.5, 0.5, 0.5, 1).
magenta	Magenta. RGBA is (1, 0, 1, 1).
red	Solid red. RGBA is (1, 0, 0, 1).
white	Solid white. RGBA is (1, 1, 1, 1).
yellow	Yellow. RGBA is (1, 0.92, 0.016, 1), but the color is nice to look at!

var colors : Color[] = {Color.red, Color.cyan, Color.yellow }; //etc

	switch (slerpColor)  {
		case 0: GUI.color = Color.black; break;
		case 1: GUI.color = Color.white; break;
		case 2: GUI.color = Color.grey; break;
		case 3: GUI.color = Color.blue; break;
		case 4: GUI.color = Color.cyan; break;
		case 5: GUI.color = Color.green; break;
		case 6: GUI.color = Color.magenta; break;
		case 7: GUI.color = Color.red; break;
		case 8: GUI.color = Color.yellow; break;
	}
				//print("scolor="+scolor);
	//GUI.color = Color.scolor;
	//Black=0,White=1,Grey=2,Blue=3,Cyan=4,
	//Green=5,Magenta=6,Red=7,Yellow=8
	
	print("GUIcolor="+GUI.color);
		*/
}



//=================



