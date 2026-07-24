using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class FurnitureSpawner : MonoBehaviour
{
    public async UniTask<bool> SpawnFurniture(string furnitureId)
    {
        FurnitureData furnitureData = GameDataManager.Inst.GetData<FurnitureData>(furnitureId);

        if (furnitureData == null)
        {
            Debug.LogWarning($"FurnitureSpawner - FurnitureData 없음 : {furnitureId}");
            return false;
        }

        RoomAssignmentService roomService = GameManager.Inst.Services.RoomAssignmentService;

        if (roomService == null)
        {
            Debug.LogWarning("FurnitureSpawner - RoomAssignmentService 없음");
            return false;
        }

        long roomInstanceId = roomService.GetAssignedRoomInstanceId(furnitureData.HeroId);

        if (roomInstanceId <= 0)
        {
            Debug.LogWarning($"FurnitureSpawner- 배정된 방 없음 : {furnitureData.HeroId}");
            return false;
        }

        BuildGridView buildGridView = ObjectManager.Inst.BuildGridView;

        if (buildGridView == null)
        {
            Debug.LogWarning("FurnitureSpawner - BuildGridView 없음");
            return false;
        }

        if (buildGridView.TryGetRoomObject(roomInstanceId, out GameObject roomObject) == false)
        {
            Debug.LogWarning($"FurnitureSpawner - 방 오브젝트 없음 : {roomInstanceId}");
            return false;
        }

        RoomFurniturePoints points = roomObject.GetComponent<RoomFurniturePoints>();

        if (points == null)
        {
            Debug.LogWarning($"FurnitureSpawner - RoomFurniturePoints 없음 : {roomObject.name}");
            return false;
        }

        Transform installPoint = points.GetPoint(furnitureData.InstallPointId);

        if (installPoint == null)
        {
            Debug.LogWarning($"FurnitureSpawner - 설치 포인트 없음 : {furnitureData.InstallPointId}");
            return false;
        }

        string prefabAddress = $"Prefab/Furniture/{furnitureData.ID}";

        GameObject prefab = await ResourceManager.Inst.LoadAsset<GameObject>(prefabAddress);

        if (prefab == null)
        {
            Debug.LogWarning($"FurnitureSpawner - 가구 프리팹 없음 : {prefabAddress}");
            return false;
        }

        CameraController cameraController = UIManager.Inst.CameraController;

        if (cameraController != null)
        {
            await cameraController.FocusFurnitureAsync(installPoint.position);
        }

        GameObject furnitureObject = Instantiate(prefab, installPoint.position, installPoint.rotation, installPoint);

        furnitureObject.name = $"{furnitureData.ID}";

        Vector3 originalScale = furnitureObject.transform.localScale;

        furnitureObject.transform.localScale = Vector3.zero;

        furnitureObject.transform.DOScale(originalScale, 0.3f).SetEase(Ease.OutBack);

        await UniTask.Delay(300);

        if (cameraController != null)
        {
            await UniTask.Delay(cameraController.GetFurnitureShowDurationMilliseconds());

            await cameraController.RestoreFurnitureFocusAsync();
        }

        return true;
    }
}