# AkiFramework

This is a code collection that i really used in my games, will be update through my development. 

I supposed these code are useful but also not-must-be-included, which is my personal understanding of "Game Framework" should be, so that you can pick code you like and use in your project for free.

## Features

### Resource System
Extra packed of addressable system, since i hate reference two assembly (`Unity.ResourceManager` && `Unity.Addressables`) when using `AsyncOperationHandle`. 

Now using ``ResourceHandle`` instead of ``AsyncOperationHandle``, can auto release asset using framework's unregister framework.

``ResourceCacheSystem<T>`` is a useful base class to load and cache gamePlay assets only for needed if you already know asset's address.

`ResourceAsyncSequence` is a sequence to wait all load task completed and invoke a callBack.

* Should install ``Unity.Addressable`` in Unity Package Manager to use ResourceSystem.

#
### Tasks
An advanced version of [Unity Timer](https://github.com/akbiggs/UnityTimer), I extend it with more feature.

Timer is now playing as a call back task, and `AkiTask` can be used for customized need, they all implement ``ITask`` interface

You may meet this kind of problem when doing job-like work: want to do a task later but this task may also be cancelled later, so a simple callBack may not satisfy well.

In ```Kurisu.Framework.Tasks```, using ``JobHandle`` to track a task and cancel it if you want.

```C#
# Example
private JobHandle handle;

private void CreateTask()
{
    handle = Task.Schedule(()=>{
        //Do something
    }, 0.5f)
}

private void CancelTask()
{
    handle.Cancel();
}
```

Thanks to [Akbiggs](https://github.com/akbiggs)!

#
### Events

Support two kinds of global event.

`AkiEvent`: Register and unregister event easily using framework's unregister framework.

```C#
# Example
private void Start()
{
    AkiEvent MyEvent = new AkiEvent();
    MyEvent.Register(()=>DoSomething()).AttachUnRegister(gameObject);
    MyEvent.Trigger();
}
```

`EventBase`: Pooled global event, idea from Unity UIElement event system.

```C#
# Example
private void SendReactionEvent()
{
    player.EventHandler.Send<ReactionEvent>(ReactionEvent.GetPooled());
}

private void RegisterReactionEventCallBack()
{
    player.EventHandler.RegisterCallbackWithUnRegister<ReactionEvent>((reactionEvent)=>{
        //Do something in callBack
    });
}
```

Thanks to [LiangXie](https://github.com/liangxiegame)!

#
### Animator

Dynamic change animator controller may cause some weird effects and we can not blend them.

Use ``VirtualAnimator`` to crossFade animator controller! Based on Unity Playable API.

```C#
# VirtualAnimator API
//CrossFade external animatorController from real controller
void Play(RuntimeAnimatorController animatorController, float fadeInTime = 0.25f);
//CrossFade external animatorController from current external animatorController
void CrossFade(RuntimeAnimatorController animatorController, float fadeInTime = 0.25f);
//CrossFade real controller from current external animatorController
void Stop(float fadeOutTime = 0.25f);
//CrossFade internal controller
void CrossFade(string stateName, float transitionTime);
//CrossFade internal controller
void Play(string stateName);
```

#
### Editor

``AssetReferenceSelector``: For pick asset and only save its reference (such as uniqueID or address)

``SubclassSelector``: Used for ``UnityEngine.SerializeReference``

Thanks to [Mackysoft](https://github.com/mackysoft/)!

#
### Others
IOC, Singleton or ObjectPool are three important tools that often used in any game project, here are my favorite version

More features can be found !

Thanks to [LiangXie](https://github.com/liangxiegame) and [Joker](https://learn.u3d.cn/u/joker-ksn)!

## License Reference

1. ``Kurisu.Framework.Tasks`` is an advanced version of [Unity Timer](https://github.com/akbiggs/UnityTimer) which is under MIT license

2. ``AkiEvent`` is a personal version modified from ```EasyEvent``` [QFramework](https://github.com/liangxiegame/QFramework) which is under MIT license

3. ``AssetReferenceSelector`` is a copy from [Unity-SerializeReferenceExtensions](https://github.com/mackysoft/Unity-SerializeReferenceExtensions) which is under MIT license