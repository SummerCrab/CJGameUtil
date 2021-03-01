//特效对象池管理器
//1.特效对象池化，单例访问，内容***随场景销毁***
//2.特效对象在激活状态回收，暂存在世界坐标极远处，减少开关GameObject带来的GC
//3.特效主要以Unity的ParticalSystem为主要载体，同时可以挂载AudioSource播放声音
//4.声音效果也可以单独池化（sfx）
//5.可以在产生特效时加入持续时间参数达到自动回收效果
//6.特效回收时不会立即转移，会等待粒子播放完成以后才完全回收，也可传入参数强制立即回收

//使用范例：
//1.在场景确保加载完成以后调用  FxPool.Instance.PoolVfx("xxxx的Prefab缓存或者实例");
//2.在需要产生特效的时候调用    FxPool.Instance.SpawnVfx("xxxx的名字"); 返回一个GameObject对象
//3.在需要消除特效的时候调用    FxPool.Instance.RecycleVfx(xxxx.gameObject); 把第二步中的GameObject对象传入，完成回收

// Copyright (c) 2020-2025 BianfengTechnology 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[LazyInit]
public class FxPool //: Singleton<FxPool>
{
    //------缓存准备------

    #region 静态
    enum FxType
    {
        vfx = 0,//Version Effects 视觉特效分类
        sfx = 1,//Sound Effects 声音效果分类
    }

    public static bool UseEffect = true; //特效开关 用于外部标记
    #endregion

    #region 缓存变量
    Dictionary<string, Transform> _fxTransMap = new Dictionary<string, Transform>();//特效对象缓存的节点
    Dictionary<string, GameObject> _fxCacheMap = new Dictionary<string, GameObject>();//特效对象缓存的预设
    Dictionary<string, System.Func<GameObject>> _fxLoadMap = new Dictionary<string, System.Func<GameObject>>();//特效对象缓存的加载

    Dictionary<string, System.Func<AudioClip>> _sfxLoadMap = new Dictionary<string, System.Func<AudioClip>>();//和特效分开，尽可能避免重名问题;
    Dictionary<string, Transform> _sfxTransMap = new Dictionary<string, Transform>();//音效对象缓存的节点
    Dictionary<string, AudioClip> _sfxCacheMap = new Dictionary<string, AudioClip>();//特效对象缓存的预设
    Vector3 _hideVect = Vector3.one * (99999f); //极远处锚点，处于及远处的粒子将停止播放，未完全停止的粒子即便被相机照到，占用像素极小
    GameObject _vfxParent;//视觉特效缓存节点
    GameObject _sfxParent;//声音效果缓存节点
    #endregion

    #region 属性访问
    /// <summary>
    /// 视觉效果实例化对象的根节点，会自动生成，便于及时访问
    /// </summary>
    public GameObject VfxParent
    {
        get
        {
            if (!_vfxParent)
            {
                _vfxParent = CreateParent(FxType.vfx);
            }
            return _vfxParent;
        }
    }
    /// <summary>
    /// 声音效果实例化对象的根节点，会自动生成，便于及时访问
    /// </summary>
    public GameObject SfxParent
    {
        get
        {
            if (!_sfxParent)
            {
                _sfxParent = CreateParent(FxType.sfx);
            }
            return _sfxParent;
        }

    }
    #endregion

    #region 接口实现
    //public override void BeforeRestart()
    //{
    //    //throw new System.NotImplementedException();
    //}

    //public override void Init(InitCompleteCallback complete)
    //{
    //    //_vfxParent = CreateParent(FxType.vfx);
    //    //_sfxParent = CreateParent(FxType.sfx);
    //    complete.Invoke(true);
    //}

    public void Dispose()
    {
        //场景切换自动清空，不需要手动释放内存
        //_fxCacheMap.Clear();
    }
    #endregion

    #region 节点生成
    void SetHide(GameObject fx)
    {
        if (fx != null)
        {
            fx.transform.position = _hideVect;
        }
    }

    bool CheckIsRecycled(GameObject fx)
    {
        return fx.transform.position.x >= _hideVect.x;
    }

    GameObject OnCreateVfx(GameObject cloneBase, Transform fxParent, string fxName, bool needHide = true)
    {
        var _cloneFx = GameObject.Instantiate(cloneBase);
        _cloneFx.name = fxName;
        _cloneFx.transform.SetParent(fxParent);
        AddCoroutine(_vfxParent, CheckDestory(_cloneFx));
        ActiveAllParticals(_cloneFx, false);
        if (needHide)
        {
            SetHide(_cloneFx);
        }
        return _cloneFx;
    }

    GameObject OnCreateSfx(AudioClip clip, Transform fxParent, string fxName, bool needHide = true)
    {
        var _cloneFx = new GameObject("Sfx_Shooter", typeof(AudioSource));
        _cloneFx.name = fxName;
        _cloneFx.transform.SetParent(fxParent);
        AddCoroutine(_sfxParent, CheckDestory(_cloneFx));
        var sr = _cloneFx.GetComponent<AudioSource>();
        sr.clip = clip;
        sr.Stop();
        if (needHide)
        {
            SetHide(_cloneFx);
        }
        return _cloneFx;
    }

    GameObject CreateParent(FxType fxType)
    {
        return new GameObject(fxType.ToString());
    }

    Transform CreateMapParent(string fxPrefabCache)
    {
        Transform fx_parent = null;
        if (_fxTransMap.TryGetValue(fxPrefabCache, out fx_parent))
        {
            if (fx_parent == null)
            {
                //节点被清除以后重新生成
                fx_parent = _fxTransMap[fxPrefabCache] = new GameObject(fxPrefabCache).transform;
            }
        }
        else
        {
            fx_parent = new GameObject(fxPrefabCache).transform;
            _fxTransMap.Add(fxPrefabCache, fx_parent);
        }
        return fx_parent;
    }

    Transform CreateMapParent(string fxPrefabCache, FxType type)
    {
        Transform fx_parent = null;
        if (GetTransDic(type).TryGetValue(fxPrefabCache, out fx_parent))
        {
            if (fx_parent == null)
            {
                //节点被清除以后重新生成
                fx_parent = GetTransDic(type)[fxPrefabCache] = new GameObject(fxPrefabCache).transform;
            }
        }
        else
        {
            fx_parent = new GameObject(fxPrefabCache).transform;
            GetTransDic(type).Add(fxPrefabCache, fx_parent);
        }
        return fx_parent;
    }


    Dictionary<string, Transform> GetTransDic(FxType type)
    {
        switch (type)
        {
            case FxType.vfx:
                return _fxTransMap;
            case FxType.sfx:
                return _sfxTransMap;
            default:
                break;
        }
        return _fxTransMap;
    }

    void CacheFx(GameObject fxPrefabCache)
    {
        GameObject fx = null;
        if (_fxCacheMap.TryGetValue(fxPrefabCache.name, out fx))
        {
            _fxCacheMap[fxPrefabCache.name] = fxPrefabCache;
        }
        else
        {
            _fxCacheMap.Add(fxPrefabCache.name, fxPrefabCache);
        }

    }

    GameObject GetFxCache(string fxName)
    {
        GameObject fx = null;
        if (!_fxCacheMap.TryGetValue(fxName, out fx))
        {
            if (_fxLoadMap.ContainsKey(fxName))
            {
                fx = _fxLoadMap[fxName]();
            }
        }
        return fx;
    }

    AudioClip GetClipCache(string fxName)
    {
        AudioClip _sfx = null;
        if (!_sfxCacheMap.TryGetValue(fxName, out _sfx))
        {
            if (_sfxLoadMap.ContainsKey(fxName))
            {
                _sfx = _sfxLoadMap[fxName]();
            }
        }
        return _sfx;
    }

    #endregion

    //------初始化准备------

    #region 池化方法
    //------------------------特效--------------------------

    /// <summary>
    /// 主要方法 缓存一个特效对象 注意特效名字不能重复
    /// </summary>
    /// <param name="fxPrefabCache"></param>
    /// <param name="cacheCount"></param>
    public void PoolVfx(GameObject fxPrefabCache, int cacheCount = 5)
    {
        CacheFx(fxPrefabCache);
        Transform fx_parent = CreateMapParent(fxPrefabCache.name);
        if (!fx_parent)
        {
            //线程保护 瞬切场景导致父节点被清除 此时也不需要继续初始化对象池
            return;
        }

        fx_parent.SetParent(VfxParent.transform);
        fx_parent.gameObject.SetActive(false);

        for (int i = 0; i < cacheCount; i++)
        {
            var clone_fx = GameObject.Instantiate(fxPrefabCache, fx_parent);
            ActiveAllParticals(clone_fx, false);
            clone_fx.name = fxPrefabCache.name;
            //clone_fx.gameObject.SetActive(false);
            SetHide(clone_fx);
        }

        if (!fx_parent.gameObject.activeSelf)
        {
            fx_parent.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 重载的同步加载方法，会记录传入的加载函数，用于特效丢失是备用加载。
    /// </summary>
    /// <param name="fxPrefabLoadCallback"></param>
    /// <param name="cacheCount"></param>
    public void PoolVfx(System.Func<GameObject> fxPrefabLoadCallback, int cacheCount = 5)
    {
        var fx = fxPrefabLoadCallback();
        if (_fxLoadMap.ContainsKey(fx.name))
        {
            _fxLoadMap[fx.name] = fxPrefabLoadCallback;
        }
        else
        {
            _fxLoadMap.Add(fx.name, fxPrefabLoadCallback);
        }
        PoolVfx(fx, cacheCount);
    }

    GameObject ExtentVfxPool(string fxName, int cacheCount = 5)
    {
        try
        {
            Transform fx_parent = CreateMapParent(fxName);
            var fx_prefab = GetFxCache(fxName);
            var clone_base = OnCreateVfx(fx_prefab, fx_parent, fxName, false);
            for (int i = 0; i < cacheCount; i++)
            {
                OnCreateVfx(clone_base, fx_parent, fxName);
            }
            return clone_base;
        }
        catch (System.Exception)
        {
            Debug.LogError(fxName);
            Debug.LogError(CreateMapParent(fxName).childCount);
            throw;
        }

    }
    GameObject ExtentSfxPool(string fxName, int cacheCount = 5)
    {
        try
        {
            Transform fx_parent = CreateMapParent(fxName, FxType.sfx);
            var fx_prefab = GetClipCache(fxName);
            var clone_base = OnCreateSfx(fx_prefab, fx_parent, fxName, false);
            for (int i = 0; i < cacheCount; i++)
            {
                OnCreateSfx(fx_prefab, fx_parent, fxName);
            }
            return clone_base;
        }
        catch (System.Exception)
        {
            Debug.LogError(fxName);
            Debug.LogError(CreateMapParent(fxName).childCount);
            throw;
        }

    }

    //-----------------------声效--------------------------

    public void PoolSfx(AudioClip clip, int cacheCount = 5)
    {
        Transform fx_parent = CreateMapParent(clip.name, FxType.sfx);
        for (int i = 0; i < cacheCount; i++)
        {
            var ad = new GameObject(clip.name).AddComponent<AudioSource>();
            ad.transform.SetParent(fx_parent);
            SetHide(ad.gameObject);
            ad.Stop();
            ad.clip = clip;
        }
    }

    public void PoolSfx(System.Func<AudioClip> clipLoadCallback, int cacheCount = 5)
    {
        var _clip = clipLoadCallback();
        if (_sfxLoadMap.ContainsKey(_clip.name))
        {
            _sfxLoadMap[_clip.name] = clipLoadCallback;
        }
        else
        {
            _sfxLoadMap.Add(_clip.name, clipLoadCallback);
        }

        PoolSfx(_clip, cacheCount);
    }

    #endregion

    //------主要功能------

    #region 特效
    /// <summary>
    /// 主要方法 通过特效名称获取池子中一个可用对象，需要通过PoolVfx方法缓存ab (重要，不要改变该节点的父对象)
    /// </summary>
    /// <param name="fxName"></param>
    /// <param name="remainTime"></param>
    /// <param name="maxRemain"></param>
    /// <returns></returns>
    public GameObject SpawnVfx(string fxName)
    {
        Transform fx_pool = null;
        GameObject found = null;

        //从缓存中寻找同名节点
        if (_fxTransMap.TryGetValue(fxName, out fx_pool))
        {
            //遍历节点下所有缓存的特效
            foreach (Transform fx in fx_pool)
            {
                //检查是否时可用状态，获取一个已经被回收，或者可用的特效缓存
                if (CheckIsRecycled(fx.gameObject))
                {
                    found = fx.gameObject;

                    //激活该特效下所有粒子及声音源
                    ActiveAllParticals(found);

                    return found;
                }
            }
        }

        if (found == null)
        {
            //如果没有可以使用的粒子，将会扩充备用特效库存，同时返回一个可用特效
            var ex = ExtentVfxPool(fxName);
            ActiveAllParticals(ex.gameObject);
            return ex.gameObject;
        }

        return null;
    }
    /// <summary>
    /// 主要方法 通过特效名称获取池子中一个可用对象，需要通过PoolVfx方法缓存ab
    /// </summary>
    /// <param name="fxName"></param>
    /// <param name="remainTime"></param>
    /// <param name="maxRemain"></param>
    /// <returns></returns>
    public GameObject SpawnVfx(string fxName, float remainTime = -1f, float maxRemain = 9f)
    {
        GameObject fx = SpawnVfx(fxName);
        if (fx)
        {
            if (remainTime > 0)
                AddCoroutine(_vfxParent, AutoRecycleVfx(fx, remainTime, maxRemain));
        }
        return fx;
    }
    /// <summary>
    /// 回收一个特效，需要残留效果传入false参数，否则立刻回收
    /// </summary>
    /// <param name="vfx"></param>
    /// <param name="isImmediant"></param>
    public void RecycleVfx(GameObject vfx, bool isImmediant = true)
    {
        if (!vfx) return;

        ActiveAllParticals(vfx, false);
        if (isImmediant)
        {
            HideVfxImmediant(vfx);
        }
        else
        {
            AddCoroutine(_vfxParent, CheckAllPsStop(vfx, HideVfxImmediant));
        }
    }

   

    #endregion

    #region 声效

    public GameObject SpawnSfx(string fxName)
    {
        Transform fx_pool = null;
        GameObject found = null;

        //从缓存中寻找同名节点
        if (GetTransDic(FxType.sfx).TryGetValue(fxName, out fx_pool))
        {
            //遍历节点下所有缓存的特效
            foreach (Transform fx in fx_pool)
            {
                //检查是否时可用状态，获取一个已经被回收，或者可用的特效缓存
                if (CheckIsRecycled(fx.gameObject))
                {
                    found = fx.gameObject;

                    //激活该特效下所有粒子及声音源
                    ActiveSounds(fx.GetComponents<AudioSource>(), true);
                    ActiveSounds(fx.GetComponentsInChildren<AudioSource>(), true);

                    return found;
                }
            }
        }

        if (found == null)
        {
            //如果没有可以使用的粒子，将会扩充备用特效库存，同时返回一个可用特效
            var ex = ExtentSfxPool(fxName);
            return ex.gameObject;
        }

        return null;
    }
    /// <summary>
    /// 主要方法 通过特效名称获取池子中一个可用对象，需要通过PoolVfx方法缓存ab
    /// </summary>
    /// <param name="fxName"></param>
    /// <param name="remainTime"></param>
    /// <param name="maxRemain"></param>
    /// <returns></returns>
    public GameObject SpawnSfx(string fxName, float remainTime = -1f, float maxRemain = 9f)
    {
        GameObject fx = SpawnSfx(fxName);
        if (fx)
        {
            if (remainTime > 0)
                AddCoroutine(_sfxParent, AutoRecycleSfx(fx, remainTime, maxRemain));
        }
        return fx;
    }
    public void RecycleSfx(GameObject sfx, bool isImmediant = true)
    {
        if (!sfx) return;
        if (isImmediant)
        {
            ActiveSounds(sfx.GetComponents<AudioSource>(), false);
            ActiveSounds(sfx.GetComponentsInChildren<AudioSource>(), false);
            HideSfxImmediant(sfx);
        }
        else
        {
            AddCoroutine(_vfxParent, CheckAllAcStop(sfx, HideSfxImmediant));
        }
    }

    #region MISC
   
    #endregion

    #endregion

    #region MISC

    WaitForSeconds _oneSecChecker = new WaitForSeconds(1f);//短时计时器
    WaitForSeconds _fiveSecChecker = new WaitForSeconds(3f);//长时计时器

    IEnumerator AutoRecycleVfx(GameObject vfx, float remainTime, float maxRemain)
    {
        WaitForSeconds checker = new WaitForSeconds(remainTime < maxRemain ? remainTime : maxRemain);
        yield return checker;
        RecycleVfx(vfx, false);
    }

    IEnumerator AutoRecycleSfx(GameObject sfx, float remainTime, float maxRemain)
    {
        WaitForSeconds checker = new WaitForSeconds(remainTime < maxRemain ? remainTime : maxRemain);
        yield return checker;
        RecycleSfx(sfx, false);
    }

    IEnumerator CheckAllPsStop(GameObject vfx, System.Action<GameObject> OnStoppedHandler)
    {
        var p1 = vfx.GetComponents<ParticleSystem>();
        var p2 = vfx.GetComponentsInChildren<ParticleSystem>();

        bool isAllPsStopped = false;
        bool isPs1Stopped = false;
        bool isPs2Stopped = false;

        while (!isAllPsStopped)
        {
            if (!vfx) yield break;
            isPs1Stopped = true;
            for (int i = 0; i < p1.Length; i++)
            {
                if (p1[i].IsAlive(true))
                {
                    isPs1Stopped = false;
                    break;
                }
            }

            isPs2Stopped = true;
            for (int i = 0; i < p2.Length; i++)
            {
                if (p2[i].IsAlive(true))
                {
                    isPs2Stopped = false;
                    break;
                }
            }


            isAllPsStopped = isPs1Stopped & isPs2Stopped;
            yield return _oneSecChecker;
        }

        if (OnStoppedHandler != null)
            OnStoppedHandler(vfx);
    }
    IEnumerator CheckAllAcStop(GameObject sfx, System.Action<GameObject> OnStoppedHandler)
    {
        var p1 = sfx.GetComponents<AudioSource>();
        var p2 = sfx.GetComponentsInChildren<AudioSource>();

        bool isAllPsStopped = false;
        bool isPs1Stopped = false;
        bool isPs2Stopped = false;

        while (!isAllPsStopped)
        {
            if (!sfx) yield break;
            isPs1Stopped = true;
            for (int i = 0; i < p1.Length; i++)
            {
                if (p1[i].loop)
                {
                    p1[i].loop = false;
                }

                if (p1[i].isPlaying)
                {
                    isPs1Stopped = false;
                    break;
                }
            }

            isPs2Stopped = true;
            for (int i = 0; i < p2.Length; i++)
            {
                if (p2[i].loop)
                {
                    p2[i].loop = false;
                }

                if (p2[i].isPlaying)
                {
                    isPs2Stopped = false;
                    break;
                }
            }


            isAllPsStopped = isPs1Stopped & isPs2Stopped;
            yield return _oneSecChecker;
        }

        if (OnStoppedHandler != null)
            OnStoppedHandler(sfx);
    }

    IEnumerator CheckDestory(GameObject fx)
    {
        //Debug.LogError(vfx.GetInstanceID());
        int count = 2;
        while (fx)
        {
            if (CheckIsRecycled(fx))
                count--;
            else
                count++;


            if (count < 0)
            {
                GameObject.Destroy(fx);
                yield break;
            }
            yield return _fiveSecChecker;
        }
    }

    MonoBehaviour _tempCoro;
    void AddCoroutine(GameObject container, IEnumerator coro)
    {
        if (!container) return;
        if (!container.TryGetComponent(out _tempCoro)) _tempCoro = container.AddComponent<MonoBehaviour>();
        _tempCoro.StartCoroutine(coro);
    }

    void HideVfxImmediant(GameObject vfx)
    {
        SetHide(vfx);
        if (vfx)
        {
            var parent = VfxParent.transform.Find(vfx.name);
            if (parent)
            {
                vfx.transform.SetParent(VfxParent.transform.Find(vfx.name));
            }
            else
            {
                GameObject.Destroy(vfx);
            }
            //vfx.transform.SetAsFirstSibling();
        }
    }

    void HideSfxImmediant(GameObject sfx)
    {
        SetHide(sfx);
        if (sfx)
        {
            var parent = SfxParent.transform.Find(sfx.name);
            if (parent)
            {
                sfx.transform.SetParent(VfxParent.transform.Find(sfx.name));
            }
            else
            {
                GameObject.Destroy(sfx);
            }
            //vfx.transform.SetAsFirstSibling();
        }
    }

    public void ActiveAllParticals(GameObject fx, bool active = true)
    {
        ActiveParticals(fx.GetComponents<ParticleSystem>(), active);
        ActiveParticals(fx.GetComponentsInChildren<ParticleSystem>(), active);

        ActiveSounds(fx.GetComponents<AudioSource>(), active);
        ActiveSounds(fx.GetComponentsInChildren<AudioSource>(), active);
    }

    void ActiveParticals(ParticleSystem[] ps, bool active = true)
    {
        for (int i = 0; i < ps.Length; i++)
        {
            if (active)
                ps[i].Play();
            else
                ps[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    void ActiveSounds(AudioSource[] ps, bool active = true)
    {
        for (int i = 0; i < ps.Length; i++)
        {
            if (active)
                ps[i].Play();
            else
                ps[i].Stop();
        }
    }

    #endregion
}
