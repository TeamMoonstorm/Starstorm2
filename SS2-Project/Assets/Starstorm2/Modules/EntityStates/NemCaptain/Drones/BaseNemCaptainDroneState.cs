using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;
using UnityEngine.UI;

namespace EntityStates.NemCaptainDrone
{
    public class BaseNemCaptainDroneState : BaseState
    {
        //private GameObject energyIndicatorContainer;

        protected TeamFilter teamFilter;
        private ProxyInteraction interactionComponent;
        protected virtual string GetContextString(Interactor activator)
        {
            return null;
        }

        protected virtual Interactability GetInteractability(Interactor activator)
        {
            return Interactability.Disabled;
        }
        protected virtual void OnInteractionBegin(Interactor activator)
        { }

        protected virtual bool ShouldShowOnScanner()
        {
            return false;
        }
        protected virtual bool ShouldIgnoreSpherecaseForInteractability (Interactor activator)
        {
            return false;
        }
        
        private string GetContextStringInternal(ProxyInteraction proxyInteraction, Interactor activator)
        {
            return GetContextString(activator);
        }

        private Interactability GetInteractabilityInternal(ProxyInteraction proxyInteraction, Interactor activator)
        {
            return GetInteractability(activator);
        }

        private void OnInteractionBeginInternal(ProxyInteraction proxyInteraction, Interactor activator)
        {
            OnInteractionBegin(activator);
        }

        private bool ShouldIgnoreSPherecastForInteractabilityInternal(ProxyInteraction proxyInteraction, Interactor activator)
        {
            return ShouldIgnoreSpherecaseForInteractability(activator);
        }

        private bool ShouldShowOnScannerInternal(ProxyInteraction proxyInteraction)
        {
            return ShouldShowOnScanner();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            teamFilter = GetComponent<TeamFilter>();
            interactionComponent = GetComponent<ProxyInteraction>();
            interactionComponent.getContextString = new Func<ProxyInteraction, Interactor, string>(GetContextStringInternal);
            interactionComponent.getInteractability = new Func<ProxyInteraction, Interactor, Interactability>(GetInteractabilityInternal);
            interactionComponent.onInteractionBegin = new Action<ProxyInteraction, Interactor>(OnInteractionBeginInternal);
            interactionComponent.shouldShowOnScanner = new Func<ProxyInteraction, bool>(ShouldShowOnScannerInternal);
            interactionComponent.shouldIgnoreSpherecastForInteractability = new Func<ProxyInteraction, Interactor, bool>(ShouldIgnoreSPherecastForInteractabilityInternal);
        }

        public override void OnExit()
        {
            interactionComponent.getContextString = null;
            interactionComponent.getInteractability = null;
            interactionComponent.onInteractionBegin = null;
            interactionComponent.shouldShowOnScanner = null;
            interactionComponent.shouldIgnoreSpherecastForInteractability = null;
            base.OnExit();
        }
    }
}
