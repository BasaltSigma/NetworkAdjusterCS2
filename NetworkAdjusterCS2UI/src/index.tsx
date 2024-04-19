import { ModRegistrar } from "cs2/modding";
import { NetworkAdjusterButton } from 'mods/adjuster-tool-ui';

const register: ModRegistrar = (moduleRegistry) => {
    // add our new button to the main toolbar
    moduleRegistry.extend("game-ui/game/components/toolbar/top/toggles.tsx", "PhotoModeToggle", NetworkAdjusterButton);
    console.log(moduleRegistry);
}

export default register;