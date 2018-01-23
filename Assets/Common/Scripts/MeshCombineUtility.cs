using UnityEngine;
using System.Collections.Generic;

/*
Attach this script as a parent to some game objects. The script will then combine the meshes at startup.
This is useful as a performance optimization since it is faster to render one big mesh than many small meshes. See the docs on graphics performance optimization for more info.

Different materials will cause multiple meshes to be created, thus it is useful to share as many textures/material as you can.
*/
//[ExecuteInEditMode] //Remove this to make merge not permanent after pressing play
public class MeshCombineUtility : MonoBehaviour{
	public MeshFilter myMeshFilter;
	Mesh mesh;
	void Awake(){
//		AdvancedMerge();
		BasicMerge();
	}
	public void AdvancedMerge(){
		// All our children (and us)
		MeshFilter[] filters = GetComponentsInChildren<MeshFilter>(false);

		// All the meshes in our children (just a big list)
		List<Material> materials = new List<Material>();
		MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(false); // <-- you can optimize this
		foreach(MeshRenderer rendererI in renderers){
			if(rendererI.transform == transform) continue;
			Material[] localMats = rendererI.sharedMaterials;
			foreach(Material localMat in localMats)
				if(!materials.Contains(localMat))
					materials.Add(localMat);
		}

		// Each material will have a mesh for it.
		List<Mesh> submeshes = new List<Mesh>();
		foreach (Material material in materials){
			// Make a combiner for each (sub)mesh that is mapped to the right material.
			List<CombineInstance> combiners = new List<CombineInstance>();
			foreach (MeshFilter filter in filters){
				if (filter.transform == transform) continue;
				// The filter doesn't know what materials are involved, get the renderer.
				MeshRenderer rendererI = filter.GetComponent<MeshRenderer>();  // <-- (Easy optimization is possible here, give it a try!)
				if (rendererI == null){
					Debug.LogError (filter.name + " has no MeshRenderer");
					continue;
				}

				// Let's see if their materials are the one we want right now.
				Material[] localMaterials = rendererI.sharedMaterials;
				for (int materialIndex = 0; materialIndex < localMaterials.Length; materialIndex++){
					if (localMaterials [materialIndex] != material) continue;
					// This submesh is the material we're looking for right now.
					CombineInstance ci = new CombineInstance();
					ci.mesh = filter.sharedMesh;
					ci.subMeshIndex = materialIndex;
					ci.transform = Matrix4x4.identity;
					combiners.Add (ci);
				}
			}
			// Flatten into a single mesh.
			Mesh mesh = new Mesh();
			mesh.CombineMeshes (combiners.ToArray(), true);
			submeshes.Add(mesh);
		}

		// The final mesh: combine all the material-specific meshes as independent submeshes.
		List<CombineInstance> finalCombiners = new List<CombineInstance>();
		foreach (Mesh meshI in submeshes){
			CombineInstance ci = new CombineInstance();
			ci.mesh = meshI;
			ci.subMeshIndex = 0;
			ci.transform = Matrix4x4.identity;
			finalCombiners.Add(ci);
		}
		Mesh finalMesh = new Mesh();
		finalMesh.CombineMeshes (finalCombiners.ToArray(), false);
		myMeshFilter.sharedMesh = finalMesh;
		Debug.Log ("Final mesh has " + submeshes.Count + " materials.");
	}
	public void BasicMerge(){
		mesh = myMeshFilter.sharedMesh;
		if(mesh == null){
			mesh = new Mesh();
			myMeshFilter.sharedMesh = mesh;
		} else{
			mesh.Clear();
		}

		MeshFilter[] filters = GetComponentsInChildren<MeshFilter>(false);
		Debug.Log("Merging " + (filters.Length - 1) + " meshes...");

		List<CombineInstance> combiners = new List<CombineInstance>();

		foreach(MeshFilter filter in filters){
			if(filter == myMeshFilter)
				continue;
			CombineInstance ci = new CombineInstance();
			ci.mesh = filter.sharedMesh;
			ci.subMeshIndex = 0;
			ci.transform = Matrix4x4.identity;
			combiners.Add(ci);
		}
//		Mesh finalMesh = new Mesh();
//		finalMesh.CombineMeshes(combiners.ToArray(), true);
//		myMeshFilter.sharedMesh = finalMesh;
		mesh.CombineMeshes(combiners.ToArray(), true);
	}
}
