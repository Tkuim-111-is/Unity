using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class USB_Controller : MonoBehaviour
{
    [Header("USB �z������")]
    public Rigidbody[] usbParts; // �u�� body, cap, tail

    [Header("�_�ϮĪG")]
    public ParticleSystem smokeHead;

    [Header("�z���Ѽ�")]
    public float explosionForce = 300f;
    public float explosionRadius = 1.5f;
    public Transform explosionCenter;

    [Header("�S��/����")]
    public GameObject explosionEffect;
    public AudioClip explosionSound;

    public ExplosionController explosionController;
    public GameObject Congratulation;
    public VoiceToGPT VoiceToGPT;


    private bool hasExploded = false;

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        foreach (var part in usbParts){
            part.isKinematic = false;
            part.AddExplosionForce(explosionForce, explosionCenter.position, explosionRadius);
            part.AddTorque(Random.insideUnitSphere * 50f);
        }

        if (smokeHead) smokeHead.Play();

        if (explosionEffect)
            Instantiate(explosionEffect, explosionCenter.position, Quaternion.identity);

        if (explosionSound)
            AudioSource.PlayClipAtPoint(explosionSound, explosionCenter.position);

        StartCoroutine(DelayedFade());
    }

    private IEnumerator DelayedFade()
    {
        yield return new WaitForSeconds(3f);
        Congratulation.SetActive(true); 
        StartCoroutine(VoiceToGPT.Context6_Ending());
    }
}
