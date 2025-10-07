using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class RescueCount : MonoBehaviour
{
    public static RescueCount instance;

    [Header("UI")]
    public TextMeshProUGUI counterText;
    public int TotalHostages = 3;

    private float endDelay = 2f;
    private int rescuedHostages = 0;
    void Awake()
    {
        if(instance == null) 
            instance = this;
        else 
            Destroy(gameObject);

        UpdateUI(); // text updates
    }

   public void addRescuedHostage()
    {
        rescuedHostages++;
        UpdateUI();

        if (rescuedHostages >= TotalHostages)
        {
            Debug.Log("All hostages rescued! Good ending!");
            Invoke(nameof(loadGoodEnd), endDelay); // load good ending after delay
        }
    }

   private void UpdateUI()
   {
       counterText.text = $"Hostages saved: {rescuedHostages}/{TotalHostages}"; // text displaying number of rescued hostages
   }

   private void loadGoodEnd()
    {
        SceneManager.LoadScene(1);
    }
}
