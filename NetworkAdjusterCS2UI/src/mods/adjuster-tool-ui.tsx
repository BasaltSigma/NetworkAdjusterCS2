import { useState } from "react";
import { ButtonTheme, Button, ConfirmationDialog, Panel, FloatingButton, PanelSection, PanelSectionRow } from "cs2/ui";
import { bindValue, trigger, useValue } from "cs2/api";
import { game, tool, Theme } from "cs2/bindings";
import { getModule, ModuleRegistryExtend } from "cs2/modding";
import mod from "../../mod.json";

export const NAT_ToolEnabled$ = bindValue<boolean>(mod.id, 'NAT_ToolEnabled', false);
export const NAT_ShowUI$ = bindValue<boolean>(mod.id, 'NAT_ShowUI', false);

const ToolBarButtonTheme: Theme | any = getModule("game-ui/game/components/toolbar/components/feature-button/toolbar-feature-button.module.scss", "classes");

const ToolBarTheme: Theme | any = getModule("game-ui/game/components/toolbar/toolbar.module.scss", "classes");

import natIcon from "./NAT_Icon.png";

export function toggle_NATToolEnabled() {
    console.log("NetworkAdjuster tool icon clicked");
    trigger(mod.id, 'NAT_ToolEnabled');
}

export const NetworkAdjusterButton: ModuleRegistryExtend = (Component) => {
    return (props) => {
        const { children, ...otherProps } = props || {};
        const NAT_ToolEnabled = useValue(NAT_ToolEnabled$);
        return (
            <>
                <Button>
                    src={natIcon}
                    className={ToolBarButtonTheme.button}
                    variant="icon"
                    selected={NAT_ToolEnabled}
                    onSelect={toggle_NATToolEnabled}
                </Button>
                <div className={ToolBarTheme.divider}></div>
                <Component {...otherProps}></Component>
            </>
        );
    }
}