import { Tabs } from 'expo-router';
import {
  CalendarDays,
  CircleUserRound,
  Compass,
  Heart,
  House,
} from 'lucide-react-native';
import type { ReactNode } from 'react';
import {
  type StyleProp,
  StyleSheet,
  Text,
  View,
  type ViewStyle,
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { Theme } from '@/shared/themes/Theme';

type DisabledTabButtonProps = {
  accessibilityLabel?: string;
  children?: ReactNode;
  style?: StyleProp<ViewStyle>;
  testID?: string;
};

type TabLabelProps = {
  focused: boolean;
  label: string;
};

function DisabledTabButton({
  accessibilityLabel,
  children,
  style,
  testID,
}: DisabledTabButtonProps) {
  return (
    <View
      accessibilityLabel={accessibilityLabel}
      accessibilityRole="button"
      accessibilityState={{ disabled: true }}
      style={style}
      testID={testID}
    >
      {children}
    </View>
  );
}

function TabLabel({ focused, label }: TabLabelProps) {
  return (
    <View style={styles.labelContainer}>
      <Text style={[styles.label, focused && styles.activeLabel]}>{label}</Text>
      <View style={[styles.activeIndicator, !focused && styles.hiddenIndicator]} />
    </View>
  );
}

export default function TabLayout() {
  const { bottom } = useSafeAreaInsets();
  const safeBottom = Math.max(bottom, 8);

  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: Theme.colors.primaryNavy,
        tabBarInactiveTintColor: Theme.colors.disabled,
        tabBarHideOnKeyboard: true,
        tabBarItemStyle: styles.tabItem,
        tabBarStyle: [
          styles.tabBar,
          {
            height: 56 + safeBottom,
            paddingBottom: safeBottom,
          },
        ],
      }}
    >
      <Tabs.Screen
        name="index"
        options={{
          title: 'Início',
          tabBarIcon: ({ color, focused }) => (
            <House
              color={color}
              fill={focused ? Theme.colors.primaryNavy : 'none'}
              size={22}
              strokeWidth={1.9}
            />
          ),
          tabBarLabel: ({ focused }) => <TabLabel focused={focused} label="Início" />,
        }}
      />
      <Tabs.Screen
        name="calendar"
        options={{
          title: 'Calendário',
          tabBarIcon: ({ color, focused }) => (
            <CalendarDays
              color={focused ? Theme.colors.white : color}
              fill={focused ? Theme.colors.primaryNavy : 'none'}
              size={22}
              strokeWidth={1.9}
            />
          ),
          tabBarLabel: ({ focused }) => (
            <TabLabel focused={focused} label="Calendário" />
          ),
        }}
      />
      <Tabs.Screen
        name="discover"
        options={{
          title: 'Descobrir',
          tabBarButton: (props) => <DisabledTabButton {...props} />,
          tabBarIcon: ({ color, focused }) => (
            <Compass
              color={focused ? Theme.colors.white : color}
              fill={focused ? Theme.colors.primaryNavy : 'none'}
              size={22}
              strokeWidth={1.9}
            />
          ),
          tabBarLabel: ({ focused }) => (
            <TabLabel focused={focused} label="Descobrir" />
          ),
        }}
      />
      <Tabs.Screen
        name="favorites"
        options={{
          title: 'Favoritos',
          tabBarButton: (props) => <DisabledTabButton {...props} />,
          tabBarIcon: ({ color, focused }) => (
            <Heart
              color={color}
              fill={focused ? Theme.colors.primaryNavy : 'none'}
              size={22}
              strokeWidth={1.9}
            />
          ),
          tabBarLabel: ({ focused }) => (
            <TabLabel focused={focused} label="Favoritos" />
          ),
        }}
      />
      <Tabs.Screen
        name="profile"
        options={{
          title: 'Perfil',
          tabBarButton: (props) => <DisabledTabButton {...props} />,
          tabBarIcon: ({ color, focused }) => (
            <CircleUserRound
              color={focused ? Theme.colors.white : color}
              fill={focused ? Theme.colors.primaryNavy : 'none'}
              size={22}
              strokeWidth={1.9}
            />
          ),
          tabBarLabel: ({ focused }) => <TabLabel focused={focused} label="Perfil" />,
        }}
      />
    </Tabs>
  );
}

const styles = StyleSheet.create({
  activeIndicator: {
    backgroundColor: Theme.colors.primaryGreen,
    borderRadius: 2,
    height: 3,
    marginTop: 4,
    width: 30,
  },
  activeLabel: {
    color: Theme.colors.primaryNavy,
    fontFamily: Theme.fonts.semiBold,
  },
  hiddenIndicator: {
    opacity: 0,
  },
  label: {
    color: Theme.colors.disabled,
    fontFamily: Theme.fonts.regular,
    fontSize: 10,
    lineHeight: 13,
  },
  labelContainer: {
    alignItems: 'center',
    height: 20,
  },
  tabBar: {
    backgroundColor: Theme.colors.white,
    borderTopColor: Theme.colors.softBorder,
    borderTopLeftRadius: 22,
    borderTopRightRadius: 22,
    borderTopWidth: 1,
    elevation: 12,
    paddingTop: 6,
    shadowColor: Theme.colors.darkNavy,
    shadowOffset: { width: 0, height: -4 },
    shadowOpacity: 0.08,
    shadowRadius: 10,
  },
  tabItem: {
    paddingVertical: 0,
  },
});
