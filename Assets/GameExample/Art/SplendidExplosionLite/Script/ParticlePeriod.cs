using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePeriod : MonoBehaviour
{
    
    bool ParticleActiveNow = false;
    public float PeriodTime;

    //public ParticleSystem P;
    public ParticleSystem.MainModule mainModule;

    // Start is called before the first frame update


    void Start()
    {
        mainModule = GetComponent<ParticleSystem>().main;
        if (PeriodTime == 0 ) PeriodTime = mainModule.duration / mainModule.simulationSpeed;       

        // StartCoroutine(ReStartParticle(PeriodTime));
        print("started");
    }

    // Update is called once per frame
    void Update()
    {
        if (ParticleActiveNow == false)
            StartCoroutine(ReStartParticle( PeriodTime ) );
    }

    IEnumerator ReStartParticle( float PeriodTime )
    {

        print("Checkit!!******************"+ PeriodTime);
        ParticleActiveNow = true;
        yield return new WaitForSeconds(PeriodTime);
        ParticleActiveNow = false;


        gameObject.SetActive(false);
        gameObject.SetActive(true);

    }


}
