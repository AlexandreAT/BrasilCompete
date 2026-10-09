import { iconSize } from './style';
import type { TabIconProps } from './types';
import { useTabIcon } from './useTabIcon';

export function TabIcon(props: TabIconProps) {
  const { fillColor, Icon, strokeColor } = useTabIcon(props);

  return <Icon color={strokeColor} fill={fillColor} {...iconSize} />;
}
