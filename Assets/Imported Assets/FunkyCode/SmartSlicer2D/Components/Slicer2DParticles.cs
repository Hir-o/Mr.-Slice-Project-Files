using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slicer2DParticles : MonoBehaviour {

	void Start () {
		Slicer2D slicer = GetComponent<Slicer2D>();
		if (slicer != null) {
			slicer.AddResultEvent(SliceEvent);
		}
	}
	
	void SliceEvent(Slice2D slice) {
		Slicer2DParticlesManager.Instantiate();
		
		float posZ = transform.position.z - 0.1f;
			
		foreach(List<Vector2D> pointList in slice.slices) {
			foreach(Pair2D p in Pair2D.GetList(pointList)) {
				Particle2D firstParticle = Particle2D.Create(Random.Range(0, 360), new Vector3((float)p.A.x, (float)p.A.y, posZ));
				Slicer2DParticlesManager.particlesList.Add(firstParticle);

				Particle2D lastParticle = Particle2D.Create(Random.Range(0, 360), new Vector3((float)p.B.x, (float)p.B.y, posZ));
				Slicer2DParticlesManager.particlesList.Add(lastParticle);

				Vector2 pos = p.A.ToVector2();
				while (Vector2.Distance(pos, p.B.ToVector2()) > 0.5f) {
					pos = Vector2.MoveTowards(pos, p.B.ToVector2(), 0.35f);
					Particle2D particle = Particle2D.Create(Random.Range(0, 360), new Vector3(pos.x, pos.y, posZ));
					Slicer2DParticlesManager.particlesList.Add(particle);
				}
			}
		} 
	}
}
