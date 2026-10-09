import { styled } from 'styled-components/native';

import { shadow } from '@/shared/styles/shadow';
import { Theme } from '@/shared/themes/Theme';

export const Container = styled.View`
  align-items: center;
  flex-direction: row;
  justify-content: space-between;
`;

export const Logo = styled.Image.attrs({
  resizeMode: 'contain',
})`
  height: 68px;
  width: 154px;
`;

export const NotificationButton = styled.TouchableOpacity.attrs({
  activeOpacity: 0.75,
})`
  align-items: center;
  background-color: ${Theme.colors.white};
  border-radius: 9999px;
  height: 46px;
  justify-content: center;
  width: 46px;
  ${shadow({ elevation: 5, offsetY: 4, opacity: 0.12, radius: 8 })}
`;

export const NotificationDot = styled.View`
  background-color: ${Theme.colors.primaryGreen};
  border-color: ${Theme.colors.white};
  border-radius: 7px;
  border-width: 2px;
  height: 12px;
  position: absolute;
  right: 2px;
  top: 2px;
  width: 12px;
`;

export const notificationIcon = {
  color: Theme.colors.primaryNavy,
  size: 25,
  strokeWidth: 1.9,
};
