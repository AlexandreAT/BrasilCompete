import type { BottomTabBarButtonProps } from '@react-navigation/bottom-tabs';
import { Tabs } from 'expo-router';

import { DisabledTabButton } from './components/DisabledTabButton/DisabledTabButton';
import { TabIcon } from './components/TabIcon/TabIcon';
import { TabLabel } from './components/TabLabel/TabLabel';
import { useTabRoutes } from './useTabRoutes';

function renderDisabledTabButton(props: BottomTabBarButtonProps) {
  return <DisabledTabButton {...props} />;
}

export function TabRoutes() {
  const { screenOptions, screens } = useTabRoutes();

  return (
    <Tabs screenOptions={screenOptions}>
      {screens.map((screen) => (
        <Tabs.Screen
          key={screen.name}
          name={screen.name}
          options={{
            title: screen.title,
            tabBarButton: screen.isDisabled ? renderDisabledTabButton : undefined,
            tabBarIcon: ({ color, focused }) => (
              <TabIcon color={color} focused={focused} screen={screen} />
            ),
            tabBarLabel: ({ focused }) => (
              <TabLabel focused={focused} label={screen.title} />
            ),
          }}
        />
      ))}
    </Tabs>
  );
}
