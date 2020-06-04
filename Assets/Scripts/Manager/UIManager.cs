using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;

    public static UIManager Instance
    {
        get
        {
            return instance;
        }
    }

    [SerializeField] private GameObject gameoverUI; // 게임오버UI
    [SerializeField] private Crosshair crosshair; // 크로스바UI

    [SerializeField] private Slider playerHpBar; // 플레이어 체력바
    [SerializeField] private Text healthText; // 체력 표시 Text
    [SerializeField] private Image gunImage; // 착용중인 총 이미지
    [SerializeField] private Text gunNameText; // 착용중인 총의 이름
    [SerializeField] private Text ammoText; // 현재 총의 남은 총알의 수
    [SerializeField] private Text ammoRemainText; // 보유한 전체 총알의 수

    [SerializeField] private Slider playerReloadBar; // 플레이어 재장전바
    [SerializeField] private Text reloadTimeText; // 캐스팅 시간표시 Text
    [SerializeField] private CanvasGroup ReloadCanvasGroip; // 재장전 캔버스

    [SerializeField] private Text waveText; // 현재 Wave
    [SerializeField] private Text remainingEnemiesText; // 현재 남은 몬스터 수를 알려주는 Text
    [SerializeField] private Text goldText; // 골드
    [SerializeField] private GameObject damageScreen; // 데미지 스크린   
    [SerializeField] private Text eventMassgeText; // 다음 공격을 알려주는 Text

    [SerializeField] private GameObject itemInventory;
    [SerializeField] private GameObject EquipInventory;

    [SerializeField] private GameObject hudCanvas; 
    [SerializeField] private GameObject shopInventory;

    private Slider loadingBar; // 로딩 게이지
    private Text loadingPercentge; // 현재 로딩 상황

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
    }

    public void ShowEventMassge()
    {
        eventMassgeText.text = "몬스터가 습격해옵니다!";
        // Text를 반짝이게 하는 코루틴
        CoroutineManager.Instance.Run("Blink", UIManager.Instance.Blink(eventMassgeText));
    }

    // 인벤토리UI 함수
    public void OpenInventory(bool active)
    {
        itemInventory.SetActive(active);
        EquipInventory.SetActive(active);
    }

    // 상점UI 함수
    public void OpenShop(bool active)
    {
        shopInventory.SetActive(active);
        itemInventory.SetActive(active);
    }
    
    // Player 피격시 데미지 스크린을 보여주는 코루틴
    public IEnumerator ShowDamageScreen()
    {
        damageScreen.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        damageScreen.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.05f);

        damageScreen.gameObject.SetActive(false);
    }

    // 재장전시 UI표시 함수
    public void ShowReloadUI(float reloadTime)
    {
        CoroutineManager.Instance.Run("FadeCoroutine", FadeBar());

        CoroutineManager.Instance.Run("CastingCoroutine", Progress(reloadTimeText, reloadTime));
    }

    // ReloadCanvasGroip 알파값 보간 코루틴
    public IEnumerator FadeBar()
    {
        float rate = 1.0f / 0.5f;
        float progress = 0.0f;

        while (progress <= 1.0)
        {
            ReloadCanvasGroip.alpha = Mathf.Lerp(0, 1, progress);
            progress += rate * Time.deltaTime;

            yield return null;
        }
    }

    // %로 바꿔주는 함수
    private double GetPercentage(double value, double total, int decimalplaces)
    {
        return System.Math.Round(value * 100 / total, decimalplaces);
    }

    // Progress 게이지 채우는 코루틴
    public IEnumerator Progress(Text text, float castTime)
    {
        float timePassed = Time.deltaTime;
        float rate = 1.0f / castTime;
        float progress = 0.0f;

        while (progress <= 1.0f)
        {
            UpdateProgressBar(playerReloadBar, 0, 1, progress);
            progress += rate * Time.deltaTime;

            timePassed += Time.deltaTime;

            text.text = GetPercentage(progress, 1.0f, 0) + " %";

            if (castTime - timePassed < 0) // 게이지 만땅
            {
                text.text = "100" + " %";
            }

            yield return null;
        }

        ReloadCanvasGroip.alpha = 0;
    }

    // ProgressBar 게이지 보간 함수
    public void UpdateProgressBar(Slider progress, float value, float target, float timer)
    {
        progress.value = Mathf.Lerp(value, target, timer);
    }

    // 골드 UI를 갱신하는 함수
    public void UpdateGoldText(int gold)
    {
        goldText.text = gold.ToString() + " $";
    }

    // 남은 몬스터UI를 갱신해주는 함수
    public void UpdataRemaingEnemiesText(int count)
    {
        if (count == 0)
        {
            remainingEnemiesText.text = "다음 밤 (Press 'Z')";
        }
        else
        {
            remainingEnemiesText.text = "남은 몬스터: " + count;
        }
    }

    // 현재 몇번째 Wave인지 갱신해주는 함수
    public void UpdateWaveText(int wave, bool active)
    {
        waveText.text = wave + " 번째 날";
        waveText.gameObject.SetActive(active);
    }

    // 반짝임 효과
    public IEnumerator Blink(Text text)
    {
        int count = 0;

        while (count < 3)
        {
            text.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            text.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.3f);
            count++;
        }

        text.gameObject.SetActive(false);
    }

    // 총의 이미지를 갱신하는 함수
    public void UpdateGunSprite(Sprite gunSpirte)
    {
        gunImage.sprite = gunSpirte;
    }

    // 총의 이름을 갱신하는 함수
    public void UpdateGunNameText(string gunName)
    {
        gunNameText.text = gunName;
    }

    // 남은 총알의 수를 갱신하는 함수
    public void UpdateAmmoText(int magAmmo, int remainAmmo, int ammoRemain)
    {
        ammoText.text = magAmmo + "/" + remainAmmo;
        this.ammoRemainText.text = ammoRemain + "발";

        if (ammoRemain == 0)
            this.ammoRemainText.text = "없음";
    }

    // 남은 몬스터를 갱신하는 함수
    public void UpdateEnemiesCountText(int count)
    {
        if (count == 0)
            waveText.text = "소탕 완료";
        else
            waveText.text = "현재 남은 몬스터 : " + count;
    }

    // 크로스 헤어 위치를 갱신하는 함수
    public void UpdateCrossHairPosition(Vector3 worldPosition)
    {
        crosshair.UpdatePosition(worldPosition);
    }

    // 크로스 헤어를 표시하고 숨기는 함수
    public void SetActiveCrosshair(bool active)
    {
        crosshair.SetActiveCrosshair(active);
    }

    // 플레이어의 체력 UI를 갱신하는 함수
    public void UpdateHealthText(float health, float maxHealth)
    {
        healthText.text = Mathf.Floor(health).ToString() + "/" + Mathf.Floor(maxHealth).ToString();

        playerHpBar.maxValue = maxHealth;
        playerHpBar.value = health;
        //Mathf.Lerp(playerHpBar.value, health, Time.deltaTime);
    }

    // 게임오버 Text 를 갱신하는 함수
    public void SetActiveGameoverUI(bool active)
    {
        gameoverUI.SetActive(active);
    }

    // 게임 재시작 함수
    public void GameRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


}