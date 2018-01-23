//Allows multiple SceneView cameras in the editor to be setup to follow gameobjects.
//October 2012 - Joshua Berberick

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class SceneViewCameraFollow : MonoBehaviour
{
	#if UNITY_EDITOR
	
	public bool on = true;
	public bool onlyInPlayMode = false;
	public SceneViewFollower[] sceneViewFollowers;
	private ArrayList sceneViews;
	
	void LateUpdate()
	{
		if(sceneViewFollowers != null && sceneViews != null)
		{
			foreach(SceneViewFollower svf in sceneViewFollowers)
			{
				if(svf.targetTransform == null) svf.targetTransform = transform;
				svf.size = Mathf.Clamp(svf.size, .01f, float.PositiveInfinity);
				svf.sceneViewIndex = Mathf.Clamp(svf.sceneViewIndex, 0, sceneViews.Count-1);
			}
		}
		
		if(Application.isPlaying)
			Follow();
	}
	
	public void OnDrawGizmos()
	{
		if(!Application.isPlaying)
			Follow();
	}
	
	void Follow()
	{
		sceneViews = UnityEditor.SceneView.sceneViews;
		if(sceneViewFollowers == null || !on || sceneViews.Count == 0) return;
		
		foreach(SceneViewFollower svf in sceneViewFollowers)
		{	
			if(!svf.enable) continue;
			UnityEditor.SceneView sceneView = (UnityEditor.SceneView) sceneViews[svf.sceneViewIndex];
			if(sceneView != null)
			{
				if((Application.isPlaying && onlyInPlayMode) || !onlyInPlayMode)
				{
					sceneView.orthographic = svf.orthographic;
					sceneView.LookAtDirect(svf.targetTransform.position + svf.positionOffset, (svf.enableFixedRotation) ? Quaternion.Euler(svf.fixedRotation) : svf.targetTransform.rotation * Quaternion.AngleAxis(svf.rotationOffset.x, svf.targetTransform.right) * Quaternion.AngleAxis(svf.rotationOffset.y, svf.targetTransform.up) * Quaternion.AngleAxis(svf.rotationOffset.z, svf.targetTransform.forward), svf.size);	
				}
			}
		}	
	}
	
	[System.Serializable]
	public class SceneViewFollower
	{
		public bool enable;
		public Vector3 positionOffset;
		public Vector3 rotationOffset;
		public bool enableFixedRotation;
		public Vector3 fixedRotation;
		public Transform targetTransform;
		public float size;
		public bool orthographic;
		public int sceneViewIndex;
		
		SceneViewFollower()
		{
			enable = false;
			positionOffset = Vector3.zero;
			enableFixedRotation = false;
			fixedRotation = Vector3.zero;
			size = 5;
			orthographic = true;
			sceneViewIndex = 0;
		}
	}
	
	#endif
}