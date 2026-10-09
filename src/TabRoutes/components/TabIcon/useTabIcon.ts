import { iconColors } from './style';
import type { TabIconProps } from './types';

export function useTabIcon({ color, focused, screen }: TabIconProps) {
  const fillColor = focused ? iconColors.focusedFill : iconColors.unfocusedFill;
  const strokeColor =
    focused && screen.whiteStrokeWhenFocused ? iconColors.focusedStroke : color;

  return { fillColor, Icon: screen.icon, strokeColor };
}
