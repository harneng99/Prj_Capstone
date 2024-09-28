using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistance
{
    void LoadData(GameData data);
    void SaveData(GameData data);
    // 저장할 데이터를 담고 있거나 저장된 데이터를 불러와야하는 class에서 상속한다.
}
