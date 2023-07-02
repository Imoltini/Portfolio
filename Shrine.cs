using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class Shrine : MonoBehaviour
{
    int pOneLayer = 25;
    int pTwoLayer = 26;
    int startingCost = 50;
    string shrineAchievement = "ACH_SHRINE";
    string wageringShrineDescription = "<color=#FFFF>COST :</color> 50 ESSENCE";
    //
    int holderEndPosition = 0;
    int holderStartPosition = -3;
    int obeliskStartPosition = 10;
    float obeliskEndPosition = -1.3f;
    int obeliskAnimationDuration = 4;
    //
    float appearPitch = 1;
    float minPitch = 0.93f;
    float maxPitch = 1.03f;
    float sfxVolume = 0.9f;
    float appearVolume = 0.85f;
    //
    bool flashing;
    int emissionID;
    bool everActivated;
    int effectDuration = 3;
    float emissionStrength = 3.2f;
    int emissionFlashDuration = 2;
    float maxFlashIntensity = 1.5f;
    //
    public Renderer rend;
    public Renderer bottomRend;
    //
    public GameObject prompt;
    public GameObject costTooltip;
    public TextMeshProUGUI costText;
    //
    public ShrineBase[] shrine;
    public string[] shrineName;
    public string[] shrineDescription;
    public Color[] shrineEmissionColor;
    //
    public Transform obelisk;
    public Transform obeliskHolder;
    public SphereCollider triggerColl;
    public FloatAndSpinObject floatScript;
    //
    public AudioSource source;
    public AudioClip appearSFX;
    public AudioClip interactSFX;
    //
    public TextMeshProUGUI promptName;
    public ParticleSystem appearEffect;
    public ParticleSystem glitterEffect;
    public TextMeshProUGUI promptDescription;
    //
    [HideInInspector] public int currentCost;
    [HideInInspector] public int timesNoDropped;
    [HideInInspector] public int timesDroppedItem;
    //
    [HideInInspector] public bool isActive;
    [HideInInspector] public bool canInteract;
    [HideInInspector] public bool taskIsActive;
    [HideInInspector] public bool taskCompleted;
    [HideInInspector] public ShrineType shrineType;
    [HideInInspector] public bool spawnedHealthBefore;

    //

    void Awake() => emissionID = Shader.PropertyToID("_EmissionColor");
    void Update()
    {
        if (!canInteract || !GM.i.isFocused) return;
        if (GM.i.systemInput.GetButtonDown(0) || GM.i.pTwoInput.GetButtonDown(0)) InteractWithShrine();
    }
    //
    public void SetShrine(int typeIndex)
    {
        GM.i.tutorialManager.ChangeToSpecificDuty(DutyType.HowToShrine);
        SetVariables(typeIndex);
        SetVisualsPositions();
        SetBoolsAndPrompt();
    }
    //
    void InteractWithShrine()
    {
        if (GM.i.tooltipManager.recentlyPickedUpItem) return;
        if (GM.i.tooltipManager.tooltipController[0].onScreen || GM.i.tooltipManager.generalTooltipOnScreen) return;
        //
        if (!everActivated) CheckForAchievement();
        GM.i.camShake.ShakeOnce();
        //
        if (taskCompleted)
        {
            isActive = false;
            canInteract = false;
            glitterEffect.Stop();
            prompt.SetActive(false);
            triggerColl.enabled = false;
            shrine[(int)shrineType].HandOutReward();
            StartCoroutine(ReduceEmissionIntensity());
            GM.i.tutorialManager.ReturnToCampaignDuties();
        }
        else
        {
            shrine[(int)shrineType].StartTask();
            if (!flashing) StartCoroutine(FlashEmissionIntensity());
            //
            if (shrineType != ShrineType.Wagering)
            {
                prompt.SetActive(false);
                PlaySoundEffect(interactSFX, Random.Range(minPitch, maxPitch), sfxVolume);
            }
        }
    }
    //
    void OnTriggerEnter(Collider hit)
    {
        if (taskIsActive) return;
        if (hit.gameObject.layer == pOneLayer || hit.gameObject.layer == pTwoLayer)
        {
            canInteract = true;
            prompt.SetActive(true);
        }
    }
    //
    void OnTriggerExit(Collider hit)
    {
        if (hit.gameObject.layer == pOneLayer || hit.gameObject.layer == pTwoLayer)
        {
            canInteract = false;
            prompt.SetActive(false);
        }
    }
    //
    void SetVariables(int index)
    {
        if (spawnedHealthBefore && index == (int)ShrineType.Healing) index = Random.Range(4, shrine.Length);
        if (index == (int)ShrineType.Wagering)
        {
            costText.text = wageringShrineDescription;
            costTooltip.SetActive(true);
            currentCost = startingCost;
            timesDroppedItem = 0;
            timesNoDropped = 0;
        }
        else costTooltip.SetActive(false);
        //
        shrineType = (ShrineType)index;
        promptName.text = shrineName[index];
        promptDescription.text = shrineDescription[index];
        rend.material.SetColor(emissionID, shrineEmissionColor[index] * emissionStrength);
    }
    //
    void SetVisualsPositions()
    {
        glitterEffect.Stop();
        glitterEffect.gameObject.SetActive(false);
        //
        appearEffect.Play();
        GM.i.camShake.LowRumble();
        floatScript.StopFloating();
        PlaySoundEffect(appearSFX, appearPitch, appearVolume);
        obelisk.localPosition = Vector3.up * obeliskStartPosition;
        obeliskHolder.localPosition = Vector3.up * holderStartPosition;
        //
        obeliskHolder.DOLocalMoveY(holderEndPosition, obeliskAnimationDuration).SetEase(Ease.OutCubic);
        obelisk.DOLocalMoveY(obeliskEndPosition, obeliskAnimationDuration).SetEase(Ease.OutCubic).OnComplete(() => HandleAnimationCompletion());
    }
    //
    void SetBoolsAndPrompt()
    {
        if ((int)shrineType > 4) taskCompleted = true;
        else taskCompleted = false;
        //
        isActive = true;
        taskIsActive = false;
        prompt.SetActive(false);
        triggerColl.enabled = true;
        shrine[(int)shrineType].claimedReward = false;
        shrine[(int)shrineType].triggeredEvent = false;
    }
    //
    void HandleAnimationCompletion()
    {
        glitterEffect.gameObject.SetActive(true);
        glitterEffect.Play();
        //
        floatScript.StartFloating();
    }
    //
    public void PlaySoundEffect(AudioClip clip, float newPitch, float vol)
    {
        source.pitch = newPitch;
        source.PlayOneShot(clip, vol);
    }
    //
    IEnumerator ReduceEmissionIntensity()
    {
        float elapsedTime = 0;
        Color initialColor = rend.material.GetColor(emissionID);
        //
        while (elapsedTime < effectDuration)
        {
            elapsedTime += Time.deltaTime;
            float emissionIntensity = Mathf.Lerp(initialColor.maxColorComponent, 0f, elapsedTime / effectDuration);
            Color newColor = initialColor * emissionIntensity;
            rend.material.SetColor(emissionID, newColor);
            //
            yield return null;
        }
    }
    //
    IEnumerator FlashEmissionIntensity()
    {
        flashing = true;
        float elapsedTime = 0;
        Color initialColor = rend.material.GetColor(emissionID);
        //
        while (elapsedTime < emissionFlashDuration)
        {
            elapsedTime += Time.deltaTime;
            float emissionIntensity = Mathf.Lerp(initialColor.maxColorComponent, maxFlashIntensity, elapsedTime / emissionFlashDuration);
            Color newColor = initialColor * emissionIntensity;
            rend.material.SetColor(emissionID, newColor);
            //
            yield return null;
        }
        //
        rend.material.SetColor(emissionID, shrineEmissionColor[(int)shrineType] * emissionStrength);
        flashing = false;
    }
    //
    void CheckForAchievement()
    {
        GM.i.steamManager.UnlockAchievement(shrineAchievement);
        everActivated = true;
    }
    //
    public void DisableRenderer() => rend.enabled = bottomRend.enabled = false;
    public void EnableRenderer() => rend.enabled = bottomRend.enabled = true;
}

public enum ShrineType
{
    Essence,
    Enchanting,
    Alluring,
    Healing,
    Wagering,
    Undying,
    Devastating,
    Alacritous,
    Mysterious
}
