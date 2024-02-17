using System;
using System.Diagnostics;
using NUnit.Framework;
using gdio.unity_api;
using gdio.unity_api.v2;
using gdio.common.objects;
using System.Xml.Linq;

namespace DemoTest
{
    [TestFixture]
    public class UnitTest
    {

        //public string testmode = "IDE";
        

        //public string platform = "desktop";
       

        //These parameters can be used to override settings used to test when running from the NUnit command line
        public string testMode = TestContext.Parameters.Get("Mode", "standalone");
        public string pathToExe = TestContext.Parameters.Get("pathToExe", null); // replace null with the path to your executable as needed



        string player = "//Player[@name='PlayerAvatar0']";
        string currentAnim = "nothing";
        ApiClient api;
        ApiClient api_2;

        public bool isLow = false;
        public int hp = 0;
        public int healCount;



        [OneTimeSetUp]
        public void Connect()
        {
            try
            {
                // First we need to create an instance of the ApiClient
                api = new ApiClient();
                api_2 = new ApiClient();

                // If an executable path was supplied, we will launch the standalone game
                if (pathToExe != null)
                {
                    ApiClient.Launch(pathToExe);
                    api.Connect("localhost", 19734, false, 30);
                    api_2.Connect("localhost", 19735, false, 30);
                }

                // If no executable path was given, we will attempt to connect to the Unity editor and initiate Play mode
                else if (testMode == "IDE")
                {
                    api.Connect("localhost", 19734, true, 30);
                    api_2.Connect("localhost", 19735, true, 30);
                }
                // Otherwise, attempt to connect to an already playing game
                else api.Connect("localhost", 19734, false, 30);
                     api_2.Connect("localhost", 19735, false, 30);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            api.EnableHooks(HookingObject.ALL);
            api_2.EnableHooks(HookingObject.ALL);

            api.Wait(3000);
            healWhenLow();
            healHealer();
        }

        [Test, Order(0)]
        public void PlayerOneStartLobby()
        {

            api.ClickObject(MouseButtons.LEFT, "/Untagged[@name='UI Canvas']/Untagged[@name='IP Start Button']", 30);
            api.Wait(2000);
            api.ClickObject(MouseButtons.LEFT, "//*[@name='Host IP Connection Button']", 30);
            api.Wait(4000);
            api.ClickObject(MouseButtons.LEFT, "/*[@name='NetworkSimulator']/*[@name='NetworkSimulatorUICanvas']/*[@name='NetworkSimulatorPopupPanel']/*[@name='Cancel Button']", 30);
            api.Wait(3000);

            
        }


        [Test, Order(1)]
        public void PlayerTwoJoinLobby()
        {
            api_2.ClickObject(MouseButtons.LEFT, "/*[@name='UI Canvas']/*[@name='Profile Button']", 30);
            api_2.Wait(2000);

            api_2.CallMethod("//*[@name='Profile Input Field']/fn:component('UnityEngine.UI.InputField')", "set_text", new object[] { "Gamedriver" } );
            api_2.ClickObject(MouseButtons.LEFT, "/*[@name='UI Canvas']/*[@name='ProfilePopup']/*[@name='ProfilesPanel']/*[@name='Viewport']/*[@name='ProfileList']/*[@name='ProfileListItemUI Prototype(Clone)']/*[@name='Select Profile Button']", 30);
            api_2.Wait(2000);

            api_2.ClickObject(MouseButtons.LEFT, "/Untagged[@name='UI Canvas']/Untagged[@name='IP Start Button']", 30);
            api_2.Wait(2000);
            api_2.ClickObject(MouseButtons.LEFT, "(//*[@name='JoinButton'])[2]", 30);
            api_2.Wait(4000);
            
           
            api_2.ClickObject(MouseButtons.LEFT, "//*[@name='Join IP Connection Button']", 30);
            api_2.Wait(10000);


        }


        [Test, Order(2)]
        public void SelectCharacters()
        {
            Assert.Multiple(() =>
            {


                //Hover mouse over character selection based on seat position
                api.MouseMoveToObject(" //*[@name='PlayerSeat (0)']", 30);
                api_2.MouseMoveToObject(" //*[@name='PlayerSeat (5)']", 30);
                api.Wait(3000);
                api_2.Wait(3000);

                //Select character using mouse left-click
                api.ClickObject(MouseButtons.LEFT, "//*[@name='PlayerSeat (0)']/*[@name='AnimationContainer']/*[@name='ClickInteract']", 30);
                api.WaitForObjectValue("//*[@name='ActiveBkgnd']", "@activeInHierarchy", true);
                api.WaitForEmptyInput();

                api_2.ClickObject(MouseButtons.LEFT, "//*[@name='PlayerSeat (5)']/*[@name='AnimationContainer']/*[@name='ClickInteract']", 30);
                api_2.WaitForObjectValue("//*[@name='ActiveBkgnd']", "@activeInHierarchy", true);
                api_2.WaitForEmptyInput();
                //Click READY button
                api.ClickObject(MouseButtons.LEFT, "//*[@name='Ready Btn']", 30);
                api_2.ClickObject(MouseButtons.LEFT, "//*[@name='Ready Btn']", 30);

                api.Wait(1000);

                api.ClickObject(MouseButtons.LEFT, "//*[@name='Ready Btn']", 30);

                api.Wait(3000);
                api_2.Wait(3000);
                //Close How To Play Panel
                api.ClickObject(MouseButtons.LEFT, "/*[@name='BossRoomHudCanvas']/*[@name='HowToPlayPopupPanel']/*[@name='Confirmation Button']", 30);
                api.Wait(3000);

                //Close How To Play Panel
                api_2.ClickObject(MouseButtons.LEFT, "/*[@name='BossRoomHudCanvas']/*[@name='HowToPlayPopupPanel']/*[@name='Confirmation Button']", 30);
                api_2.Wait(3000);

                api_2.SetObjectFieldValue("/Player[@name='PlayerAvatar0']/@transform", "position", new Vector3(-1, 0, 7));
            });
            
        }
     
        [Test, Order(2)]
        public void moveImp() 
        {
            int startingHp = api.GetObjectFieldValue<int>("//Player[@name='PlayerAvatar0']/fn:component('Unity.BossRoom.Gameplay.GameplayObjects.NetworkHealthState')/@HitPoints/@Value");
            api.SetObjectFieldValue("(/Untagged[@name='Imp(Clone)'])[4]/@transform", "position", placeNear("/Player[@name='PlayerAvatar0']"));
            api.Wait(5000);
            api_2.Wait(5000);
            api.CallMethod("(//*[@name='Imp(Clone)'])[4]/fn:component('Unity.BossRoom.Gameplay.GameplayObjects.Character.ServerCharacter')", "ReceiveHP", new object[] { new HPathObject("//*[@name='Imp(Clone)'])[4]/fn:component('Unity.BossRoom.Gameplay.GameplayObjects.Character.ServerCharacter')"), -15 });
            int damagedHp = api.GetObjectFieldValue<int>("//Player[@name='PlayerAvatar0']/fn:component('Unity.BossRoom.Gameplay.GameplayObjects.NetworkHealthState')/@HitPoints/@Value");

            api_2.ClickObject(MouseButtons.LEFT, "/Player[@name='PlayerAvatar0']", 30);
            api_2.Wait(1000);
            api_2.KeyPress(new KeyCode[] { KeyCode.Alpha2 }, 30);
            api.Wait(500);
            int healedHp = api.GetObjectFieldValue<int>("//Player[@name='PlayerAvatar0']/fn:component('Unity.BossRoom.Gameplay.GameplayObjects.NetworkHealthState')/@HitPoints/@Value");

            Assert.That((startingHp == healedHp) && (damagedHp < startingHp), "player did not recieve damage");

        }

        [Test, Order(3)]
        public void doSomething() 
        {
            //healWhenLow();
            api.Wait(60000);
        }
        public void healWhenLow() 
        {
      
            // HP listener
            string hitPointsListener = api.ScheduleScript(@"local hitPoints = ResolveObject(""//Player[@name='PlayerAvatar0']/fn:component('Unity.BossRoom.Gameplay.GameplayObjects.NetworkHealthState')/@HitPoints/@Value"");
                    if hitPoints < 1100 then
                        Notify(true)
                    end
                    ", ScriptExecutionMode.EveryNthFrames, (int)api.GetLastFPS() * 3);
           

            // Trigger callback when a low HP is detected
            api.ScriptSignal += (sender, args) => {
                Console.WriteLine("Hitpoints are low! Time to Heal!");
                HandleTankLowHp();
            };

            // Unschedule the SmartAgent script
            //api.UnscheduleScript(hitPointsListener);


        }

        public void healHealer() 
        {
            string hitPointsListenerHealer = api.ScheduleScript(@"local hitPoints = ResolveObject(""//Player[@name='PlayerAvatar1']/fn:component('Unity.BossRoom.Gameplay.GameplayObjects.NetworkHealthState')/@HitPoints/@Value"");
                    if hitPoints < 700 then
                        Notify(true)
                    end
                    ", ScriptExecutionMode.EveryNthFrames, (int)api.GetLastFPS() * 3);

           


            // Trigger callback when a low HP is detected
            api.ScriptSignal += (sender, args) => {
                Console.WriteLine("Healer Hitpoints are low! Time to Heal!");
                HandleHealerLowHp();
            };
        }

        

        public void HandleTankLowHp()
        {
            api.Wait(300);

            api_2.ClickObject(MouseButtons.LEFT, "/Player[@name='PlayerAvatar0']", 30);
            api_2.Wait(1000);
            api_2.KeyPress(new KeyCode[] { KeyCode.Alpha2 }, 30);
            api.Wait(500);

            
            
        }

        public void HandleHealerLowHp()
        {
            api.Wait(300);

            api_2.ClickObject(MouseButtons.LEFT, "/Player[@name='PlayerAvatar1']", 30);
            api_2.Wait(1000);
            api_2.KeyPress(new KeyCode[] { KeyCode.Alpha2 }, 30);
            api.Wait(500);

            // Reset popup flag
            isLow = false;
            healCount++;

        }
        public Vector3 placeNear(string Hpath) 
        {
            Vector3 pos = api.GetObjectPosition(Hpath);
            Vector3 newPos = new Vector3(pos.x + 3, pos.y,pos.z + 3);
            return newPos;
        }



        [OneTimeTearDown]
        public void Disconnect()
        {
            // Disconnect the GameDriver client from the agent
            
            
            api.DisableHooks(HookingObject.ALL);
            api.Wait(2000);
            api.Disconnect();
            api.Wait(2000);

           
            api_2.DisableHooks(HookingObject.ALL);
            api_2.Wait(2000);
            api_2.Disconnect();
            api_2.Wait(2000);
        }

    }
}