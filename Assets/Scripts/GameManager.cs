using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia { get; private set; }

    public enum EstadoJuego
    {
        MenuPrincipal,
        Jugando,
        Pausado,
        Victoria,
        GameOver
    }

    [Header("Estado actual")]
    [SerializeField] private EstadoJuego estadoActual = EstadoJuego.Jugando;
    public EstadoJuego EstadoActual => estadoActual;

    [Header("Maldicion - Estatuas")]
    [Tooltip("Cantidad total de estatuas que hay que destruir para romper la maldicion")]
    public int totalEstatuas = 3;
    private int estatuasDestruidas = 0;

    [Header("UI")]
    public GameObject panelMenuPrincipal;
    public GameObject panelPausa;
    public GameObject panelVictoria;
    public GameObject panelGameOver;
    public bool pausarAlTerminar = true;

    [Header("Eventos")]
    public UnityEvent alRomperMaldicion;
    public UnityEvent alGanar;
    public UnityEvent alPerder;
    public UnityEvent alPausar;
    public UnityEvent alReanudar;

    private void Awake()
    {
        // Singleton
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        // DontDestroyOnLoad(gameObject); // descomenta si querés persistir entre escenas
    }

    private void Start()
    {
        // UI inicial
        SetAllPanelsInactive();
        if (panelMenuPrincipal != null)
        {
            panelMenuPrincipal.SetActive(false);
        }
        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(false);

        estadoActual = EstadoJuego.Jugando;
        estatuasDestruidas = 0;
    }

    void SetAllPanelsInactive()
    {
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(false);
    }

    // ---------------------------
    // Métodos públicos que llaman otros scripts
    // ---------------------------

    // Llamado por EstatuaMaldita cuando se destruye una estatua
    public void EstatuaDestruida()
    {
        if (estadoActual != EstadoJuego.Jugando) return;

        estatuasDestruidas++;
        Debug.Log($"GameManager: estatua destruida {estatuasDestruidas}/{totalEstatuas}");

        if (estatuasDestruidas >= totalEstatuas)
        {
            RomperMaldicionYGanar();
        }
    }

    // Llamado por PlayerControllerFPS cuando el jugador muere
    public void JugadorMurio()
    {
        if (estadoActual != EstadoJuego.Jugando) return;

        estadoActual = EstadoJuego.GameOver;

        if (panelGameOver != null) panelGameOver.SetActive(true);
        if (pausarAlTerminar) Time.timeScale = 0f;

        alPerder?.Invoke();
    }

    // ---------------------------
    // Acciones internas
    // ---------------------------
    private void RomperMaldicionYGanar()
    {
        estadoActual = EstadoJuego.Victoria;

        // Ejemplo: hacer de dia, eliminar enemigos, etc. (puedes personalizar)
        HacerDeDia();
        EliminarEnemigos();

        if (panelVictoria != null) panelVictoria.SetActive(true);
        if (pausarAlTerminar) Time.timeScale = 0f;

        alRomperMaldicion?.Invoke();
        alGanar?.Invoke();
    }

    private void HacerDeDia()
    {
        // Implementa cambios de ambiente si los tenes (luz, skybox, etc.)
        // Este método es un placeholder; en tu GameManager original tenías luzSol y skybox.
    }

    private void EliminarEnemigos()
    {
        // Elimina todos los objetos con tag "Enemigo" si lo deseas
        // (asegurate de que tus enemigos tengan ese tag)
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemigo");
        foreach (GameObject e in enemigos)
        {
            Destroy(e);
        }
        Debug.Log($"GameManager: enemigos eliminados: {enemigos.Length}");
    }

    // ---------------------------
    // Pausa / Menu / Control de escena
    // ---------------------------

    public void PausarJuego()
    {
        if (estadoActual != EstadoJuego.Jugando) return;
        estadoActual = EstadoJuego.Pausado;
        Time.timeScale = 0f;
        if (panelPausa != null) panelPausa.SetActive(true);
        alPausar?.Invoke();
    }

    public void ReanudarJuego()
    {
        if (estadoActual != EstadoJuego.Pausado) return;
        estadoActual = EstadoJuego.Jugando;
        Time.timeScale = 1f;
        if (panelPausa != null) panelPausa.SetActive(false);
        alReanudar?.Invoke();
    }

    public void AbrirMenuPrincipal()
    {
        // Opcional: pausar el juego y mostrar menu principal
        estadoActual = EstadoJuego.MenuPrincipal;
        Time.timeScale = 0f;
        SetAllPanelsInactive();
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(true);
    }

    public void VolverAlJuegoDesdeMenu()
    {
        // Cierra menu principal y reanuda juego
        estadoActual = EstadoJuego.Jugando;
        Time.timeScale = 1f;
        if (panelMenuPrincipal != null) panelMenuPrincipal.SetActive(false);
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CargarEscena(string escenaNombre)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaNombre);
    }

    public void SalirJuego()
    {
        // En editor no hace nada visible; en build cierra la app
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
