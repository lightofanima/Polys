﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polys.Game.States;
using Polys.Util;
using Polys;

namespace Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestUtils()
        {
            //Clamp
            Assert.AreEqual(Util.Clamp(-1, -1, -1), -1);
            Assert.AreEqual(Util.Clamp(-2, -1, 2), -1);
            Assert.AreEqual(Util.Clamp(4, -4, -2), -2);
            Assert.AreEqual(Util.Clamp(1, -1, 2), 1);

            //Memset
            byte[] arr1 = new byte[123];
            byte[] arr2 = new byte[1];
            byte[] arr3 = null;

            Util.MemSet(arr1, 3);
            Util.MemSet(arr2, 3);
            Util.MemSet(arr3, 3);

            foreach (byte b in arr1)
                Assert.AreEqual(b, 3);
            foreach (byte b in arr2)
                Assert.AreEqual(b, 3);

            //Is Rect Visible
            Assert.AreEqual(Maths.isRectVisible(new Rect(0, 0, 0, 0), 100, 100), true);
            Assert.AreEqual(Maths.isRectVisible(new Rect(99, 99, 2333, 400), 100, 100), true);
            Assert.AreEqual(Maths.isRectVisible(new Rect(-2, -3, 1, 1), 100, 100), false);
            Assert.AreEqual(Maths.isRectVisible(new Rect(0, 0, 1, 1), 1, 1), true);
            Assert.AreEqual(Maths.isRectVisible(new Rect(200, 330, 1, 5), 100, 100), false);

            //Fit rectangle inside rectangle projection
            OpenGL.Vector4 bottomLeftVec = new OpenGL.Vector4(-1, -1, 0, 1);
            OpenGL.Vector4 projected = bottomLeftVec * Maths.matrixFitRectIntoScreen(100, 200, 50, 60);
            Assert.IsTrue(bottomLeftVec.x >= -1 && bottomLeftVec.x <= 1 && bottomLeftVec.y >= -1 && bottomLeftVec.y <= 1);
            projected = bottomLeftVec * Maths.matrixFitRectIntoScreen(100, 200, 50, 60);
            Assert.IsTrue(bottomLeftVec.x >= -1 && bottomLeftVec.x <= 1 && bottomLeftVec.y >= -1 && bottomLeftVec.y <= 1);
            projected = bottomLeftVec * Maths.matrixFitRectIntoScreen(200, 65, 50, 60);
            Assert.IsTrue(bottomLeftVec.x >= -1 && bottomLeftVec.x <= 1 && bottomLeftVec.y >= -1 && bottomLeftVec.y <= 1);
            projected = bottomLeftVec * Maths.matrixFitRectIntoScreen(100, 200, 80, 20);
            Assert.IsTrue(bottomLeftVec.x >= -1 && bottomLeftVec.x <= 1 && bottomLeftVec.y >= -1 && bottomLeftVec.y <= 1);
            projected = bottomLeftVec * Maths.matrixFitRectIntoScreen(500, 200, 70, 2);
            Assert.IsTrue(bottomLeftVec.x >= -1 && bottomLeftVec.x <= 1 && bottomLeftVec.y >= -1 && bottomLeftVec.y <= 1);
            projected = bottomLeftVec * Maths.matrixFitRectIntoScreen(123, 123, 123, 123);
            Assert.IsTrue(bottomLeftVec.x >= -1 && bottomLeftVec.x <= 1 && bottomLeftVec.y >= -1 && bottomLeftVec.y <= 1);
            projected = bottomLeftVec * Maths.matrixFitRectIntoScreen(35, 2, 1, 60);
            Assert.IsTrue(bottomLeftVec.x >= -1 && bottomLeftVec.x <= 1 && bottomLeftVec.y >= -1 && bottomLeftVec.y <= 1);

            //Bigger power of two
            Assert.AreEqual(Maths.biggerPowerOfTwo(3), 4);
            Assert.AreEqual(Maths.biggerPowerOfTwo(4), 4);
            Assert.AreEqual(Maths.biggerPowerOfTwo(0), 0);
            Assert.AreEqual(Maths.biggerPowerOfTwo(4765849035765432L), 9007199254740992L);
        }

        //A state for testing purposes.
        class TestState : Polys.Game.States.State
        {
            public static List<int> stateUpdateOrder = new List<int>();
            public static List<int> stateDrawOrder = new List<int>();

            public int value;

            public TestState(int v) { value = v; }

            public void Dispose() { }

            public void setStateManager(StateManager m) { }

            public StateManager.StateRenderResult draw()
            {
                stateDrawOrder.Add(value);
                if (value > 5)
                    return StateManager.StateRenderResult.StopDrawing;
                else
                    return StateManager.StateRenderResult.Continue;
            }

            public StateManager.StateUpdateResult updateAfterFrame()
            {
                stateUpdateOrder.Add(value);
                return StateManager.StateUpdateResult.UpdateBelow;
            }

            public StateManager.StateUpdateResult updateAfterInput()
            {
                stateUpdateOrder.Add(value);
                if (value > 2)
                    return StateManager.StateUpdateResult.UpdateBelow;
                else if (value > 1)
                    return StateManager.StateUpdateResult.Finish;
                else
                    return StateManager.StateUpdateResult.Quit;
            }

            public StateManager.StateUpdateResult updateBeforeInput()
            {
                stateUpdateOrder.Add(value);
                if (value > 4)
                    return StateManager.StateUpdateResult.UpdateBelow;
                else if (value < 2)
                    return StateManager.StateUpdateResult.Finish;
                else
                    return StateManager.StateUpdateResult.Quit;
            }
        }

        [TestMethod]
        public void TestStates()
        {
            StateManager manager = new StateManager(new TestState(0));
            manager.push(new TestState(1));
            Assert.AreEqual(((TestState)manager.pop()).value, 1);
            Assert.AreEqual(((TestState)manager.top).value, 0);
            manager.push(new TestState(1));
            manager.push(new TestState(2));
            manager.push(new TestState(3));
            manager.push(new TestState(4));
            manager.push(new TestState(5));
            manager.push(new TestState(6));
            manager.push(new TestState(7));

            Assert.AreEqual(manager.update(StateManager.UpdateType.BeforeInput), false);
            Assert.IsTrue(Enumerable.SequenceEqual(TestState.stateUpdateOrder, new List<int>() { 7, 6, 5, 4 }));
            TestState.stateUpdateOrder = new List<int>();

            Assert.AreEqual(manager.update(StateManager.UpdateType.AfterInput), true);
            Assert.IsTrue(Enumerable.SequenceEqual(TestState.stateUpdateOrder, new List<int>() { 7, 6, 5, 4, 3, 2 }));
            TestState.stateUpdateOrder = new List<int>();

            Assert.AreEqual(manager.update(StateManager.UpdateType.AfterFrame), true);
            Assert.IsTrue(Enumerable.SequenceEqual(TestState.stateUpdateOrder, new List<int>() { 7, 6, 5, 4, 3, 2, 1, 0 }));
            TestState.stateUpdateOrder = new List<int>();

            manager.draw();
            Assert.IsTrue(Enumerable.SequenceEqual(TestState.stateDrawOrder, new List<int>() { 0, 1, 2, 3, 4, 5, 6 }));
            TestState.stateDrawOrder = new List<int>();

            manager.pop();
            manager.pop();
            manager.pop();
            manager.draw();
            Assert.IsTrue(Enumerable.SequenceEqual(TestState.stateDrawOrder, new List<int>() { 0, 1, 2, 3, 4 }));
            TestState.stateDrawOrder = new List<int>();
        }


        /*
        class TestIntentHandler : IIntentHandler
        {
            public struct Data
            {
                public IntentManager.IntentType type;
                public bool down, up, held;
                public int id;
                public Data(IntentManager.IntentType t, bool d, bool u, bool h, int i)
                { type = t; down = d; up = u; held = h; id = i; }
            }

            public static List<Data> intentsHandled = new List<Data>();
            int id;

            public TestIntentHandler(int i) { id = i; }

            public void handleIntent(IntentManager.IntentType intentCode, bool isKeyDown, bool isKeyUp, bool isKeyHeld)
            {
                intentsHandled.Add(new Data(intentCode, isKeyDown, isKeyUp, isKeyHeld, id));
            }
        }

        
        [TestMethod]
        public void TestIntents()
        {
            
            TestIntentHandler handler1 = new TestIntentHandler(1);
            TestIntentHandler handler2 = new TestIntentHandler(2);

            IntentManager.register(handler1, IntentManager.IntentType.ESC, true, false, true);
            IntentManager.addBinding(SDL2.SDL.SDL_Keycode.SDLK_ESCAPE, IntentManager.IntentType.ESC);
            IntentManager.addBinding(SDL2.SDL.SDL_Keycode.SDLK_w, IntentManager.IntentType.WALK_UP);

            IntentManager.keyDown(SDL2.SDL.SDL_Keycode.SDLK_ESCAPE);
            IntentManager.dispatchRequestsAndClear();

            Assert.IsTrue(TestIntentHandler.intentsHandled.Count == 1 &&
                TestIntentHandler.intentsHandled[0].Equals(new TestIntentHandler.Data(IntentManager.IntentType.ESC, true, false, false, 1)));

            IntentManager.keyDown(SDL2.SDL.SDL_Keycode.SDLK_ESCAPE);
            IntentManager.dispatchRequestsAndClear();

            Assert.IsTrue(TestIntentHandler.intentsHandled.Count == 2 &&
                TestIntentHandler.intentsHandled[1].Equals(new TestIntentHandler.Data(IntentManager.IntentType.ESC, true, false, false, 1)));

            IntentManager.keyDown(SDL2.SDL.SDL_Keycode.SDLK_w);
            IntentManager.keyDown(SDL2.SDL.SDL_Keycode.SDLK_s);
            IntentManager.keyUp(SDL2.SDL.SDL_Keycode.SDLK_ESCAPE);
            IntentManager.dispatchRequestsAndClear();

            Assert.IsTrue(TestIntentHandler.intentsHandled.Count == 2 &&
                TestIntentHandler.intentsHandled[1].Equals(new TestIntentHandler.Data(IntentManager.IntentType.ESC, true, false, false, 1)));

            IntentManager.deregister(handler1);
            IntentManager.keyDown(SDL2.SDL.SDL_Keycode.SDLK_ESCAPE);
            IntentManager.dispatchRequestsAndClear();

            Assert.IsTrue(TestIntentHandler.intentsHandled.Count == 2 &&
                TestIntentHandler.intentsHandled[1].Equals(new TestIntentHandler.Data(IntentManager.IntentType.ESC, true, false, false, 1)));

            IntentManager.register(handler1, IntentManager.IntentType.ESC, true, true, false);
            IntentManager.register(handler2, IntentManager.IntentType.ESC, false, false, true);
            IntentManager.addBinding(SDL2.SDL.SDL_Keycode.SDLK_e, IntentManager.IntentType.ESC);

            IntentManager.keyDown(SDL2.SDL.SDL_Keycode.SDLK_e);
            IntentManager.dispatchRequestsAndClear();

            Assert.IsTrue(TestIntentHandler.intentsHandled.Count == 3 &&
                TestIntentHandler.intentsHandled[2].Equals(new TestIntentHandler.Data(IntentManager.IntentType.ESC, true, false, false, 1)));

            IntentManager.register(handler1, IntentManager.IntentType.WALK_UP, true, false, false);

            IntentManager.keyDown(SDL2.SDL.SDL_Keycode.SDLK_w);
            IntentManager.keyDown(SDL2.SDL.SDL_Keycode.SDLK_ESCAPE);
            IntentManager.dispatchRequestsAndClear();

            Assert.IsTrue(TestIntentHandler.intentsHandled.Count == 5 &&
                (TestIntentHandler.intentsHandled[3].Equals(new TestIntentHandler.Data(IntentManager.IntentType.WALK_UP, true, false, false, 1)) ^
                TestIntentHandler.intentsHandled[4].Equals(new TestIntentHandler.Data(IntentManager.IntentType.WALK_UP, true, false, false, 1))) &&
                (TestIntentHandler.intentsHandled[3].Equals(new TestIntentHandler.Data(IntentManager.IntentType.ESC, true, false, false, 1)) ^
                TestIntentHandler.intentsHandled[4].Equals(new TestIntentHandler.Data(IntentManager.IntentType.ESC, true, false, false, 1))));

            IntentManager.register(handler2, IntentManager.IntentType.ESC, true, false, false);
            IntentManager.keyDown(SDL2.SDL.SDL_Keycode.SDLK_ESCAPE);
            IntentManager.dispatchRequestsAndClear();

            Assert.IsTrue(TestIntentHandler.intentsHandled.Count == 7 &&
                (TestIntentHandler.intentsHandled[5].Equals(new TestIntentHandler.Data(IntentManager.IntentType.ESC, true, false, false, 2)) ^
                TestIntentHandler.intentsHandled[6].Equals(new TestIntentHandler.Data(IntentManager.IntentType.ESC, true, false, false, 2))) &&
                (TestIntentHandler.intentsHandled[5].Equals(new TestIntentHandler.Data(IntentManager.IntentType.ESC, true, false, false, 1)) ^
                TestIntentHandler.intentsHandled[6].Equals(new TestIntentHandler.Data(IntentManager.IntentType.ESC, true, false, false, 1))));

            IntentManager.register(handler1, IntentManager.IntentType.ESC, false, true, false);
            IntentManager.register(handler2, IntentManager.IntentType.ESC, false, false, true);
            IntentManager.register(handler2, IntentManager.IntentType.ESC, false, false, true);
            IntentManager.keyUp(SDL2.SDL.SDL_Keycode.SDLK_ESCAPE);
            IntentManager.keyHeld(SDL2.SDL.SDL_Keycode.SDLK_ESCAPE);
            IntentManager.dispatchRequestsAndClear();

            Assert.IsTrue(TestIntentHandler.intentsHandled.Count == 9 &&
    (TestIntentHandler.intentsHandled[7].Equals(new TestIntentHandler.Data(IntentManager.IntentType.ESC, false, true, false, 1)) ^
    TestIntentHandler.intentsHandled[8].Equals(new TestIntentHandler.Data(IntentManager.IntentType.ESC, false, true, false, 1))) &&
    (TestIntentHandler.intentsHandled[7].Equals(new TestIntentHandler.Data(IntentManager.IntentType.ESC, false, false, true, 2)) ^
    TestIntentHandler.intentsHandled[8].Equals(new TestIntentHandler.Data(IntentManager.IntentType.ESC, false, false, true, 2))));
        }
        */
    }

}
