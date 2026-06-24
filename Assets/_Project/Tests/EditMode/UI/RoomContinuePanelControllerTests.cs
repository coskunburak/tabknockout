using NUnit.Framework;
using TapKnockout.Level;
using TapKnockout.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TapKnockout.UI.Tests
{
    public sealed class RoomContinuePanelControllerTests
    {
        [Test]
        public void Show_MakesPanelVisibleAndButtonInteractable()
        {
            var fixture = CreateFixture();

            try
            {
                fixture.Controller.Show();

                Assert.That(fixture.Controller.IsVisible, Is.True);
                Assert.That(fixture.CanvasGroup.alpha, Is.EqualTo(1f));
                Assert.That(fixture.CanvasGroup.interactable, Is.True);
                Assert.That(fixture.CanvasGroup.blocksRaycasts, Is.True);
                Assert.That(fixture.Button.interactable, Is.True);
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void Hide_MakesPanelInvisibleAndButtonNotInteractable()
        {
            var fixture = CreateFixture();

            try
            {
                fixture.Controller.Show();

                fixture.Controller.Hide();

                Assert.That(fixture.Controller.IsVisible, Is.False);
                Assert.That(fixture.CanvasGroup.alpha, Is.EqualTo(0f));
                Assert.That(fixture.CanvasGroup.interactable, Is.False);
                Assert.That(fixture.CanvasGroup.blocksRaycasts, Is.False);
                Assert.That(fixture.Button.interactable, Is.False);
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [Test]
        public void SetFlowController_StoresReference()
        {
            var fixture = CreateFixture();
            var flowObject = new GameObject("Flow");

            try
            {
                var flowController = flowObject.AddComponent<ChapterRoomRewardFlowController>();

                fixture.Controller.SetFlowController(flowController);

                Assert.That(fixture.Controller.FlowController, Is.EqualTo(flowController));
            }
            finally
            {
                Object.DestroyImmediate(flowObject);
                fixture.Destroy();
            }
        }

        private static ContinuePanelFixture CreateFixture()
        {
            var root = new GameObject("RoomContinuePanel", typeof(CanvasGroup));
            var buttonObject = new GameObject("ContinueButton", typeof(Button));
            var labelObject = new GameObject("Label", typeof(Text));
            buttonObject.transform.SetParent(root.transform, false);
            labelObject.transform.SetParent(buttonObject.transform, false);
            var controller = root.AddComponent<RoomContinuePanelController>();

            var fixture = new ContinuePanelFixture
            {
                Root = root,
                CanvasGroup = root.GetComponent<CanvasGroup>(),
                Button = buttonObject.GetComponent<Button>(),
                Label = labelObject.GetComponent<Text>(),
                Controller = controller
            };

            return fixture;
        }

        private sealed class ContinuePanelFixture
        {
            public GameObject Root;
            public CanvasGroup CanvasGroup;
            public Button Button;
            public Text Label;
            public RoomContinuePanelController Controller;

            public void Destroy()
            {
                Object.DestroyImmediate(Root);
            }
        }
    }
}
