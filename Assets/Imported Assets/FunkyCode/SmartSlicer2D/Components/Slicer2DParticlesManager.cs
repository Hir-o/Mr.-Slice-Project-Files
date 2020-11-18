using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slicer2DParticlesManager : MonoBehaviour {
	public int particlesCount = 0;
	static public Slicer2DParticlesManager instance;

	static public List<Particle2D> particlesList = new List<Particle2D>();

	static public void Instantiate() {
		if (instance != null) {
			return;
		}

		GameObject manager = new GameObject();
		manager.name = "Slicer2D Particles";

		instance = manager.AddComponent<Slicer2DParticlesManager>();
	}

	void Start () {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			if (Slicer2D.Debug.enabled) {
				Debug.LogWarning("Slicer2D: Multiple Particle Managers Detected!");
			}
			Destroy(this);
		}
	}

	void Update() {
		foreach(Particle2D particle in new List<Particle2D>(particlesList)) {
			if (particle.Update() == true) {
				particle.Draw();
			} else {
				particlesList.Remove(particle);
			}
		}

		particlesCount = particlesList.Count;
	}
}
