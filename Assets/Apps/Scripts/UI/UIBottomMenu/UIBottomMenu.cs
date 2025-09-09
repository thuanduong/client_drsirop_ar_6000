using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBottomMenu : PopupEntity<UIBottomMenu.Entity>
{
    public class Entity
    {
        public UIToggleComponent.Entity toggleShop;
        public UIToggleComponent.Entity toggleQuest;
        public UIToggleComponent.Entity togglePlay;
        public UIToggleComponent.Entity toggleLeaderboard;
        public UIToggleComponent.Entity toggleCard;
        
    }

    public UIToggleComponent toggleShop;
    public UIToggleComponent toggleQuest;
    public UIToggleComponent togglePlay;
    public UIToggleComponent toggleLeaderboard;
    public UIToggleComponent toggleCard;

    protected override void OnSetEntity()
    {
        toggleQuest.SetEntity(entity.toggleQuest);
        toggleShop.SetEntity(entity.toggleShop);
        togglePlay.SetEntity(entity.togglePlay);
        toggleLeaderboard.SetEntity(entity.toggleLeaderboard);
        toggleCard.SetEntity(entity.toggleCard);
    }

}
