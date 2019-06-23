using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreParticles : MonoBehaviour
{
    public GameObject scoreParticle;

    GameObjectPool particles = null;

    // Start is called before the first frame update
    void Start()
    {
        particles = new GameObjectPool(scoreParticle, 100, gameObject);
        foreach (GameObject go in particles)
        {
            go.GetComponent<ScoreParticle>().parent = this;
        }
    }

    public void SpawnParticle(Vector3 position, int score)
    {
        GameObject particle = particles.Get();
        // need to change the pool class to either activate things when you get them or not deactivate them when you return them. this is inconsistent
        particle.SetActive(true);
        particle.GetComponent<ScoreParticle>().Spawn(position, score);
    }

    public void Done(GameObject particle)
    {
        particles.Return(particle);
    }
}
