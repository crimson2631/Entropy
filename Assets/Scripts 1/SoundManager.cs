using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public AudioClip introSound;
    


    public AudioSource loopSound;

    //Play loop sound after intro
    IEnumerator delayPlay(float time){
        yield return new WaitForSeconds(time);
        loopSound.Play();
    }

	void Start () {

        //Use length of the introSound to define when the loop should start playing
        StartCoroutine(delayPlay(introSound.length));

      
    

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
