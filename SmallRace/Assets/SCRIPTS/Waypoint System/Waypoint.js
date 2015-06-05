// WayPoint .... Mich Shire Feb. 2015

@script ExecuteInEditMode()

var isGrounded : boolean = true;
@Range (.5, 10.0) 
var tolerance : float = 5;
@Range (0.0, 1) 
var yOffset : float = .01;
private var hit : RaycastHit; 

	// Draw the waypoint pickable gizmo
function OnDrawGizmos () {
	if(isGrounded){
		if (Physics.Raycast(transform.position, -Vector3.up, hit, tolerance)) {
			var y=hit.distance; transform.position.y -=y ; //print ("y= " + y);
			transform.position.y +=yOffset;
		}
	}
	var thispos = this.transform.position;
	Gizmos.DrawIcon (thispos, "Waypoint.tif");
	Gizmos.color = Color.white;
	Gizmos.DrawWireSphere (thispos, .3);
}