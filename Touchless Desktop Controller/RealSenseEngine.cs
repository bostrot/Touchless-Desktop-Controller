using System;

namespace Desktop_Controller
{
    public class RealSenseEngine
    {
        #region Exception Class
        public class RealSenseEngineException : Exception
        {
            public RealSenseEngineException(string message) : base(message) { }
            public RealSenseEngineException(string message, params object[] args) : base(String.Format(message, args)) { }
        }
        #endregion

        #region Class Members
        private readonly PXCMSession m_session;
        private readonly PXCMSenseManager m_senseManager;
        private readonly PXCMTouchlessController m_touchlessController;
        #endregion

        public RealSenseEngine()
        {
            m_session = PXCMSession.CreateInstance();
            if (m_session == null)
            {
                throw new RealSenseEngineException("Failed to create a Session");
            }
            m_senseManager = m_session.CreateSenseManager();
            if (m_senseManager == null)
            {
                throw new RealSenseEngineException("Failed to create a SenseManager");
            }
            pxcmStatus res = m_senseManager.EnableTouchlessController();
            if (res != pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                throw new RealSenseEngineException("Failed to Enable Touchless Controller");
            }

            var handler = new PXCMSenseManager.Handler();
            res = m_senseManager.Init(handler);
            if (res != pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                throw new RealSenseEngineException("Failed to Init Handler");
            }

            // getting touchless controller
            m_touchlessController = m_senseManager.QueryTouchlessController();
            if (m_touchlessController == null)
            {
                throw new RealSenseEngineException("Failed to Query Touchless Controller");
            }
        }

        public event PXCMTouchlessController.OnFiredUXEventDelegate UXEventFired
        {
            add
            {
                pxcmStatus res = m_touchlessController.SubscribeEvent(value);
                if (res != pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    throw new RealSenseEngineException("Failed to Subscribe to Event ({0})", value.ToString());
                }
            }

            remove
            {
                pxcmStatus res = m_touchlessController.UnsubscribeEvent(value);
                if (res != pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    throw new RealSenseEngineException("Failed to UnSubscribe to Event ({0})", value.ToString());
                }
            }
        }

        public event PXCMTouchlessController.OnFiredAlertDelegate AlertFired
        {
            add
            {
                pxcmStatus res = m_touchlessController.SubscribeAlert(value);
                if (res != pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    throw new RealSenseEngineException("Failed to Subscribe to Alert ({0})", value.ToString());
                }
            }

            remove
            {
                pxcmStatus res = m_touchlessController.UnsubscribeAlert(value);
                if (res != pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    throw new RealSenseEngineException("Failed to UnSubscribe to Alert ({0})", value.ToString());
                }
            }
        }

        public void SetConfiguration(PXCMTouchlessController.ProfileInfo.Configuration configuration)
        {
            PXCMTouchlessController.ProfileInfo pInfo;
            pxcmStatus res = m_touchlessController.ClearAllGestureActionMappings();
            if (res != pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                throw new RealSenseEngineException("Failed to Clear All Gesture Action Mappings. {0}", res);
            }
            res = m_touchlessController.QueryProfile(out pInfo);
            if (res != pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                throw new RealSenseEngineException("Failed to Query Profile. {0}", res);
            }

            pInfo.config = configuration;

            res = m_touchlessController.SetProfile(pInfo);
            if (res != pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                throw new RealSenseEngineException("Failed to Set Profile. {0}", res);
            }
        }

        public void Start()
        {
            m_senseManager.StreamFrames(false);
        }

        public void Shutdown()
        {
            m_touchlessController.Dispose();
            m_senseManager.Close();
            m_session.Dispose();
        }
    }
}
