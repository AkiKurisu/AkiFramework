# Resource

Resource module is based on Addressables. 

## Features

- `ResourceSystem` Let you use Rx/Async for asset loading.

- `AudioSystem` Use poolable audio source.

- `FXSystem` Use poolable particle system.

- `SoftAssetReference` Speed up asset management workflow.


### SoftAssetReference

Original `AssetReference` need modify source code to add address processing and it use GUID as runtime asset identifier (should enable `Include Guid` in Asset Group Scheme). 

`SoftAssetReference` only use address as identifier and not ensure asset exists.

You can use `AssetReferenceConstraintAttribute` to set address processing method and the Asset Group it will use when dropping asset not managed by Addressables.