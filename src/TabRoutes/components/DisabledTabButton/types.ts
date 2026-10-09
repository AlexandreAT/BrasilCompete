import type { BottomTabBarButtonProps } from '@react-navigation/bottom-tabs';

export type DisabledTabButtonProps = Pick<
  BottomTabBarButtonProps,
  'accessibilityLabel' | 'children' | 'style' | 'testID'
>;
