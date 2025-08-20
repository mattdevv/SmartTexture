# SmartTexture Asset for Unity
SmartTexture is a custom asset for Unity that allows you a [channel packing](http://wiki.polycount.com/wiki/ChannelPacking) workflow in the editortextures and use them in the Unity editor for a streamlined workflow.
SmartTextures work as a regular 2D texture asset and you can assign it to material inspectors.

Dependency tracking is handled by SmartTexture, meaning if input textures change the SmartTexture will be re-generated automatically. The input textures are editor only dependencies, they will not be marked for inclusion in the build.

### Usecases
- Packing terrain layer mask map
- Packing smoothness into metallic alpha for URP Lit shader
- Packing mask map or detail map for HDRP lit shaders

### Why use this fork?
- Individual source channel mapping
- Output size control
- Additional texture options
- Extended texture compression settings
- Bug fixes 

<img alt="inspector" src="https://github.com/mattdevv/SmartTexture/assets/94596138/29039c40-1247-4cfa-b67d-b4ad6bf47c73">


---


## Installation
SmartTexture is a unity package and you can install it from Package Manager.

Option 1: [Install package via Github](https://docs.unity3d.com/Manual/upm-ui-giturl.html) by going to the Package Manger and installing via git URL with the link: `https://github.com/mattdevv/SmartTexture.git`

Option 2: Clone or download this Github project and [install it as a local package](https://docs.unity3d.com/Manual/upm-ui-local.html).

## How to use
1) Create a SmartTexture asset by clicking on `Asset -> Create -> Smart Texture`, or by right-clicking on the Project Window and then `Create -> Smart Texture`.
<img width="870" alt="create" src="https://user-images.githubusercontent.com/7453395/82161430-d9865100-989c-11ea-9497-19d1cf77fed9.png">

2) An asset will be created in your project.
<img width="378" alt="asset" src="https://user-images.githubusercontent.com/7453395/82161427-d68b6080-989c-11ea-9fae-1d65e06ad3d6.png">

3) On the asset inspector you can configure input textures and texture settings for your smart texture.
4) Hit `Apply` button to generate the texture with selected settings.
<img width="347" alt="inspector" src="https://github.com/mattdevv/SmartTexture/assets/94596138/693532dc-be7f-4836-bbf2-b6ec4b990f5c">

5) Now you can use this texture as any regular 2D texture in your project.
<img width="524" alt="assign" src="https://github.com/mattdevv/SmartTexture/assets/94596138/432f0b89-d9d7-46df-95ae-63bf7a8b66fc">

## To Do
- Add a 'fix now' button to warning and error messages
- More warnings (e.g. warn about non-pow-2 textures and compression)
- Add an option to convert the output image to premultiplied-alpha 
- Add an option to expand RGB colors over pixels with 0 alpha
- Try and read common image types directly avoiding the need to keep input textures uncompressed
- Special handling if an input image is also a SmartTexture so we directly read it's input texture (and avoid cycles)

## Acknowledgements
* Thanks to Felipe Lira ([phi-lira](https://github.com/phi-lira)) for the base of this project 
* Thanks to pschraut for [Texture2DArrayImporter](https://github.com/pschraut/UnityTexture2DArrayImportPipeline) project. I've used that project to learn a few things about ScriptedImporter and reused the code to create asset file. 
* Thanks to [Aras P.](https://twitter.com/aras_p) for guidance and ideas on how to create this.
