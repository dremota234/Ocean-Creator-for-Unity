using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkSystemCore : MonoBehaviour, IChunkSystem
{
    [Header("Settings")]
    // Настройки системы
    [SerializeField] private ChunkSystemSettings _settings;
    [SerializeField] private WaveSettings _waveSettings;
    [SerializeField] private Material _waterMaterial;

    [Header("Debug")]
    // Отладочные поля
    [SerializeField] private Transform _playerTransform;
    // Зависимости
    private IChunkProvider _chunkProvider;      // Управление пулом чанков
    private IChunkGenerator _chunkGenerator;    // Генерация геометрии
    private IChunkLoader _chunkLoader;          // Логика загрузки/выгрузки
    // События и состояния
    public event Action<Chunk> OnChunkCreated;
    public event Action<Vector2Int> OnChunkRemoved;

    private Coroutine _updateCoroutine;
    private HashSet<Vector2Int> _currentlyLoadedChunks = new HashSet<Vector2Int>();
    //

    private void Awake()
    {
        InitializeDependencies();
    }

    private void Start()
    {
        Initialize(); 
    }

    private void InitializeDependencies()
    {
        _chunkProvider = new ChunkPool(_settings, transform);               // Пул объектов
        _chunkGenerator = new GPUChunkGenerator(_settings, _waveSettings);  // GPU-генерация
        _chunkLoader = new DynamicChunkLoader(_settings);                   // Динамическая загрузка
    }

    public void Initialize()
    {
        UpdateWaveProperties(); // Инициализация шейдерных свойств

        if (_updateCoroutine != null)
            StopCoroutine(_updateCoroutine);
        
        _updateCoroutine = StartCoroutine(UpdateChunksCoroutine());// Запуск шняги для периодического обновления

        Debug.Log("ChunkSystem initialized successfully");
    }

    public void UpdateSystem(Vector3 playerPosition)
    {
        UpdateWaveProperties();
        UpdateChunksAroundPlayer(playerPosition);
    }
    // шняга для обновления чанков
    private IEnumerator UpdateChunksCoroutine()
    {
        while (true)
        {
            if (_playerTransform != null) // Проверяет наличие трансформа игрока
            {
                UpdateChunksAroundPlayer(_playerTransform.position);
            }

            yield return new WaitForSeconds(_settings.chunkUpdateInterval); //Использует интервал обновления из настроек
        }
    }

    private void UpdateChunksAroundPlayer(Vector3 playerPosition)
    {
        // 1. Загрузка новых чанков
        var chunksToLoad = _chunkLoader.GetChunksToLoad(playerPosition);
        foreach (var coord in chunksToLoad)
        {
            if (!_currentlyLoadedChunks.Contains(coord))
            {
                LoadChunk(coord);
                _chunkLoader.LoadChunk(coord);
                _currentlyLoadedChunks.Add(coord);
            }
        }

        // 2. Выгрузка старых чанков
        var chunksToUnload = _chunkLoader.GetChunksToUnload(playerPosition);
        foreach (var coord in chunksToUnload)
        {
            if (_currentlyLoadedChunks.Contains(coord))
            {
                UnloadChunk(coord);
                _chunkLoader.UnloadChunk(coord);
                _currentlyLoadedChunks.Remove(coord);
            }
        }

        // 3. Обновление геометрии активных чанков
        foreach (var coord in _currentlyLoadedChunks)
        {
            if (_chunkProvider.IsChunkActive(coord))
            {
                var chunk = _chunkProvider.GetChunk(coord);
                if (chunk != null)
                {
                    _chunkGenerator.UpdateChunkGeometry(chunk, _waveSettings);
                }
            }
        }
    }
    //  Процесс загрузки чанка
    private void LoadChunk(Vector2Int coord)
    {
        var chunk = _chunkProvider.GetChunk(coord); // Получаем из пула
        if (chunk == null) return;

        // Генерируем меш если его нет или при первом использовании
        if (chunk.Mesh == null)
        {
            var newChunk = _chunkGenerator.GenerateChunk(coord, _settings);

            // Копируем данные в чанк из пула    (желательно переписать чтобы было через ссылки а не копипаста!!!!!!!!!!!!!!!)
            chunk.Mesh = newChunk.Mesh;
            chunk.Vertices = newChunk.Vertices;
            chunk.Triangles = newChunk.Triangles;
            chunk.UVs = newChunk.UVs;

            if (chunk.MeshFilter != null)
            {
                chunk.MeshFilter.mesh = chunk.Mesh;  // Применяем меш к объекту
            }
        }

        // Настраиваем материал
        if (chunk.MeshRenderer != null && _waterMaterial != null)
        {
            chunk.MeshRenderer.material = _waterMaterial;
        }
        // Уведомление подписчиков №не на ютубе :(
        OnChunkCreated?.Invoke(chunk);
        ChunkEvents.RaiseChunkCreated(chunk);
    }

    private void UnloadChunk(Vector2Int coord)
    {
        _chunkProvider.ReturnChunk(coord);
        OnChunkRemoved?.Invoke(coord);
        ChunkEvents.RaiseChunkDestroyed(coord);
    }
    // Обновление свойств шейдера
    private void UpdateWaveProperties()
    {
        if (_waterMaterial != null && _waveSettings != null)
        {
            //             Базовые параметры волн
            _waterMaterial.SetFloat("_WaveSpeed", _waveSettings.waveSpeed);
            _waterMaterial.SetFloat("_WaveAmplitude", _waveSettings.waveAmplitude);
            _waterMaterial.SetFloat("_WaveFrequency", _waveSettings.waveFrequency);
            _waterMaterial.SetFloat("_WaveSteepness", _waveSettings.waveSteepness);
            //              Направления волн (поддержка до 3 направлений)
            if (_waveSettings.waveDirections.Length >= 1)
                _waterMaterial.SetVector("_WaveDirection1", _waveSettings.waveDirections[0]);
            if (_waveSettings.waveDirections.Length >= 2)
                _waterMaterial.SetVector("_WaveDirection2", _waveSettings.waveDirections[1]);
            if (_waveSettings.waveDirections.Length >= 3)
                _waterMaterial.SetVector("_WaveDirection3", _waveSettings.waveDirections[2]);
        }
    }

    public void SetWaveParameters(WaveSettings parameters)
    {
        _waveSettings = parameters;
        UpdateWaveProperties();
    }
    /*--------------------------Метод выполняет полную очистку ресурсов системы чанков
                                при завершении работы или перезагрузке. */

    
    public void Cleanup()
    {
        if (_updateCoroutine != null) 
            StopCoroutine(_updateCoroutine);  // Остановка шняги обновления

        _chunkProvider?.Cleanup();            // Очистка провайдера чанков
        _currentlyLoadedChunks.Clear();       // Очистка отслеживаемых чанков
        OnChunkCreated = null;                //  Онуление событий ;( - отписка от событий
        OnChunkRemoved = null;                //  Онуление событий ;( - отписка от событий
        // СhunkEvents всё еще могут иметь подписчиков надо ликведировать !!!!!!!!!!!!!!!!!!!
    }

    /* Unity вызывает OnDestroy() при:
     * Уничтожении GameObject 
     * Загрузке новой сцены
     * Остановке игры в редакторе
     */
    private void OnDestroy()    
    {
        Cleanup();
    }
    //----------------------------------------------------------------------------------
    // Визуализация зоны загрузки чанков в редакторе. (Ебейшая blue обводка)
    private void OnDrawGizmosSelected() 
    {
        if (_settings == null) return;

        Gizmos.color = Color.blue;
        if (_playerTransform != null)
        {
            var center = _playerTransform.position;
            var size = new Vector3(
                _settings.chunksX * _settings.chunkWorldSize,
                0.1f,
                _settings.chunksZ * _settings.chunkWorldSize
            );
            Gizmos.DrawWireCube(center, size); // DrawWireCube только контур( не заслоняет объекты)
                                               // DrawCube - сплошной куб( заполненный)
        }
    }
}