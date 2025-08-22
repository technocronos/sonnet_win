using UnityEngine.EventSystems;

//IEventSystemHandlerを継承させる
public interface RecieveDestroyInterface : IEventSystemHandler
{
    void DestroyScene();
}
