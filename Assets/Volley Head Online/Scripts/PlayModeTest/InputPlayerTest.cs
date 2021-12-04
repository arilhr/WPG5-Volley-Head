using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VollyHead.Online;
using Mirror;

public class InputPlayerTest
{
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator InputPlayerMoveTest()
    {
        GameObject player = new GameObject();
        player.AddComponent<NetworkIdentity>();
        Player playerScript = player.AddComponent<Player>();


        // player input
        // player move

        yield return null;

        // assert is player moving
    }

    [UnityTest]
    public IEnumerator InputPlayerMoveWhenNotInStateTest()
    {
        GameObject player = new GameObject();
        player.AddComponent<NetworkIdentity>();
        Player playerScript = player.AddComponent<Player>();


        // player input

        yield return null;

        // player move

        // assert player not moving
    }

    [UnityTest]
    public IEnumerator InputPlayerServeTest()
    {
        GameObject player = new GameObject();
        player.AddComponent<NetworkIdentity>();
        Player playerScript = player.AddComponent<Player>();


        // player input serve

        yield return null;

        // serve power is increase
    }

    [UnityTest]
    public IEnumerator InputPlayerServeWhenNotInStateTest()
    {
        GameObject player = new GameObject();
        player.AddComponent<NetworkIdentity>();
        Player playerScript = player.AddComponent<Player>();


        // player input serve

        yield return null;

        // serve power is zero
    }
}
