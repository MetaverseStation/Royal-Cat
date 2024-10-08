using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Net;

public class FoodObjectPool : IPunPrefabPool
{
    private Dictionary<string, Queue<GameObject>> _objPools = new Dictionary<string, Queue<GameObject>>();

    //프리팹 경로와 지정할 위치, 회전값
    public GameObject Instantiate(string prefabId, Vector3 pos, Quaternion rot)
    {
        //오브젝트의 순수 이름을 파싱한다. prefabId는 모든 경로가 포함된 이름이고 prefabName은 오브젝트 네임이다.
        prefabId = prefabId.Replace("(Clone)", "").Trim();
        string prefabName = GetPrefabName(prefabId);
        
        Debug.Log(prefabName + "생성");
        
        GameObject obj = null;

        
        if (_objPools.ContainsKey(prefabName) && _objPools[prefabName].Count > 0)
        {            
            while(_objPools[prefabName].Count > 0)
            {
                obj = _objPools[prefabName].Dequeue();

                if (obj.activeSelf)
                    continue;
                else 
                    break;
            }

            
        }
        else
        {
            obj = GameObject.Instantiate(Resources.Load<GameObject>(prefabId), pos, rot);
            GameObject obj2 = GameObject.Instantiate(Resources.Load<GameObject>(prefabId), Vector3.zero, Quaternion.identity);
            obj2.SetActive(false);
            GameObject.Destroy(obj2, 0.1f);
        }

        obj.transform.position = pos;
        obj.transform.rotation = rot;
        //obj.SetActive(true);

        return obj;
    }

    public void Destroy(GameObject obj)
    {
        Debug.Log("오브젝트 풀 사용 완료" + obj.name);
        //오브젝트를 전부 사용하고 나면 비활성화 한 뒤 다시 풀에 집어넣는다.

        obj.SetActive(false);

        string objName = obj.name;
        objName = objName.Replace("(Clone)", "").Trim();

        if (!_objPools.ContainsKey(objName))
        {
            _objPools[objName] = new Queue<GameObject>();
            
        }

        _objPools[objName].Enqueue(obj);
    }
    // '/'이후로 시작되는 문자열을 받아온다.
    private string GetPrefabName(string prefabId)
    {
        return prefabId.Substring(prefabId.LastIndexOf("/") + 1);
    }

    public void ClearObjPool()
    {
        foreach (var pool in _objPools)
        {
            pool.Value.Clear();
        }

        _objPools.Clear();
    }
}

