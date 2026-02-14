Forget Me Not
Forget Me Not is a VR game where you act as a florist in a small town. You have to stock the shelves, create bouquets and serve customers to earn money. You can also go around the town after the day is done and interact with the locals.

However after a few days, you realise that the village isn't all that it seems with the locals seemingly constantly being very forgetful. Until you reach the last day and you find out that the village was a dimentia village all along.

Design Process
We wanted to design this game so as to raise more awareness about dimentia and dimentia villages as a whole as it usually isn't talked about alot especially among the younger people and they might not understand what it's like to care for someone with dimentia. This game would then allow players to experience what interacting with those with dimentia could be like which should allow them to become more patient in the future when interacting with those that do have dimentia.

User Stories:
As a teenager with a grandparent with dimentia, I want to play the game, to understand more about the signs of dimentia.
As someone that does not know much about dimentia villages, I want to experience the game, so that I can learn more about their importance.

Figma Link:
https://www.figma.com/design/gtqqaiQUhULMp9SgAE6OKF/Forget_Me_Not?node-id=0-1&t=4NV7nlle2p6bCWq3-1

Design Rationale Doc: 
https://www.canva.com/design/DAG_kwMQtb8/eRSbYefKXcEqDWcQstWb6g/edit?utm_content=DAG_kwMQtb8&utm_campaign=designshare&utm_medium=link2&utm_source=sharebutton

Features
1. Integration with Firebase: The game is fully integrated with firebase. Dialogue and the type of flowers the NPC wants is all controlled through Firebase and can be changed without needing to completely create a new game build. Players are also able to save their player data through the firebase and can continue their games from wher ethey left off.

2. Stocking the shelves: It uses sockets to keep the flowers in position in the box as well as to snap the flowers to the container game object. Upon interacting with the container socket, it will also instantiate some of the same flowers into that container while destroying some of the flowers that were in the box.

3. Creating Bouquets: Uses an event to detect which type of flower was placed into the socket. When sockets are all filled, bringing the ribbon gameobject close to the wrapping paper will instantiate a bouquet corresponding to the flowers placed into the sockets and destroy the wrapping paper and flowers game objects.

4. Serving Customers: Created bouquets are placed into a socket representing the customers hand. It will then trigger an event where the customer will check if the item given is the same one as what was pre-set for them to want. Giving them the correct item will have them spawn the money prefab with a set amount based off of bouquet size while failing to give them the correct item just causes them to walk away.

5. Register Interactions: Created bouquets are placed into a socket representing the customers hand. It will then trigger an event where the customer will check if the item given is the same one as what was pre-set for them to want. Giving them the correct item will have them spawn the money prefab with a set amount based off of bouquet size while failing to give them the correct item just causes them to walk away.

6. Twist: After players play through the required number of days and have interacted with all NPCs in the town area atleast once, the twist sequence should happen as it marks the end of the game experience.

Bugs:
1. Boxes are hard to move: The boxes holding the flowers are consistently hard to move for unknown reason as even when checked to not be clipping into something, it will still move very slowly.

2. Flowers stocking too many flowers: When stocking flowers, there is a chance for more than the 6 wanted flowers to spawn. This also makes future gameplay when you need to create the bouquets harder as the flowers would then clip into each other making them near impossible to move.

3. Collisions not detected: some colliders like the ones for the shelf are not detecting collisions with the player/other objects and thus would not perform the action that they are meant to.

Wishlist:
1. Better story integration: To make it a more accurate representation of those with dementia as well as to make it more engaging across the days as NPCs only says the same thing every time they order flowers.

2. More variety in flowers and bouquets: To make a harder and more engaging gameplay loop, we would like a greater variety of what players can do in the game during the main loop.

Credits
Media
Unity asset packages used:
https://assetstore.unity.com/packages/3d/vegetation/the-illustrated-nature-sample-161188 The Illustrated Nature - Sample by Artkovski
https://assetstore.unity.com/packages/3d/vegetation/trees/yughues-free-palm-trees-13540 Yughues Free Palm Trees by Nobiax / Yughues
https://assetstore.unity.com/packages/3d/vegetation/plants/yughues-free-bushes-13168 Yughues Free Bushes by Nobiax / Yughues
https://assetstore.unity.com/packages/2d/textures-materials/sky/allsky-free-10-sky-skybox-set-146014 AllSky Free - 10 Sky / Skybox Set by rpgwhitelock
https://assetstore.unity.com/packages/3d/vegetation/casual-plants-lite-pack-303173 Casual Plants Lite Pack by GoGo Creator
https://assetstore.unity.com/packages/3d/vegetation/flowers/ornamental-flower-set-11920 Ornamental Flower Set by Game Asset Studio
https://github.com/omid3098/Unity-URP-GlassShader Unity Glass Shader by omid3098 & HiRoS-neko

NPC Models:
Tripo3d AI: https://studio.tripo3d.ai/?utm_source=google&utm_medium=cpc&utm_campaign=brand&utm_term=tripo%203d&gad_source=1&gad_campaignid=22867198940&gclid=Cj0KCQiA18DMBhDeARIsABtYwT0FFNY0vdCf_zGcGQWr8EkNh2sJtktgknO3fhvW0cdLxPbqdFEh4swaAjIzEALw_wcB