import {
  CalendarDays,
  CircleUserRound,
  Compass,
  Heart,
  House,
  type LucideIcon,
} from 'lucide-react-native';

export type TabScreen = {
  icon: LucideIcon;
  isDisabled: boolean;
  name: 'calendar' | 'discover' | 'favorites' | 'index' | 'profile';
  title: string;
  whiteStrokeWhenFocused: boolean;
};

export const MIN_TAB_BAR_BOTTOM_INSET = 8;

export const TAB_SCREENS: TabScreen[] = [
  {
    icon: House,
    isDisabled: false,
    name: 'index',
    title: 'Início',
    whiteStrokeWhenFocused: false,
  },
  {
    icon: CalendarDays,
    isDisabled: false,
    name: 'calendar',
    title: 'Calendário',
    whiteStrokeWhenFocused: true,
  },
  {
    icon: Compass,
    isDisabled: true,
    name: 'discover',
    title: 'Descobrir',
    whiteStrokeWhenFocused: true,
  },
  {
    icon: Heart,
    isDisabled: true,
    name: 'favorites',
    title: 'Favoritos',
    whiteStrokeWhenFocused: false,
  },
  {
    icon: CircleUserRound,
    isDisabled: true,
    name: 'profile',
    title: 'Perfil',
    whiteStrokeWhenFocused: true,
  },
];
