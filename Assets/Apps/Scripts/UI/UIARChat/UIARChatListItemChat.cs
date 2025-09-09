using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;


public class UIARChatListItemChat : UIComponentListWithTemplates<UIARChatItemChat, UIARChatItemChat.Entity>
{
    public GameObject Container;

    public override void AddEntity(int type, UIARChatItemChat.Entity item)
    {
        if (entity != null)
        {
            base.AddEntity(type, item);
        }
    }

    public async UniTask RefreshList(CancellationToken cancellationToken)
    {
        try
        {
            Container.SetActive(false);
            await UniTask.Delay(100, cancellationToken: cancellationToken);
            Container.SetActive(true);
        }
        catch { }
    }

}
