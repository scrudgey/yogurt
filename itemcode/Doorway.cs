﻿using UnityEngine;
// using System.Collections;

public class Doorway : Interactive {

	public string destination;
	public int destinationEntry;
	public int entryID;
	public bool spawnPoint = false;
	public AudioClip leaveSound;
	public AudioClip enterSound;
	protected AudioSource audioSource;
	public string leaveDesc;

	// Use this for initialization
	public virtual void Start () {
		Interaction leaveaction = new Interaction(this, "Exit", "Leave");
		interactions.Add(leaveaction);
		audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
	}
	public virtual void Enter(GameObject player){
		Vector3 tempPos = transform.position;
		tempPos.y = tempPos.y - 0.05f;
		player.transform.position = tempPos;
		PlayEnterSound();
	}	
	public void Leave(){
		// audioSource.PlayOneShot(leaveSound);
		GameManager.Instance.publicAudio.PlayOneShot(leaveSound);
		GameManager.Instance.LeaveScene(destination, destinationEntry);
	}
	public void PlayEnterSound(){
		if (audioSource == null){
			audioSource = Toolbox.Instance.SetUpAudioSource(gameObject);
		}
		if (enterSound != null){
			audioSource.PlayOneShot(enterSound);
		}
	}

	public string Leave_desc(){
		if (leaveDesc != ""){
			return leaveDesc;
		} else {
			return "Go to "+destination;
		}
	}

}
