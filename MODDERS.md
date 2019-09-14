# Modders Guide: Heelz
## Create Assets
use 3dsmax or blender and make some shit
### Blender (hobo tier)
## Setup Variables
### Default heel manifest template
```xml
	<AI_HeelsData>
		<heel id="[PROVIDE HEEL ID THAT YOU SPECIFIED IN LIST .CSV FILE]">
			<root vec="0,0.4,0"/>
			<foot01><roll vec="30,0,0" min="0,0,0" max="35,360,360"/><move vec="0,0,0"/> <scale vec="1,1,1"/></foot01>
			<foot02><roll vec="10.0,0,0" min="0,0,0" max="35,360,360"/><move vec="0,0,0"/><scale vec="1,1,1"/></foot02>
			<toes01 fixed="true"><roll vec="-40,0,0"/><move vec="0,0,0"/><scale vec="1,1,1"/></toes01>
		</heel>
	</AI_HeelsData>
```
## Load and Test
