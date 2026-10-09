import { styled } from 'styled-components/native';

import { shadow } from '@/shared/styles/shadow';
import { Theme } from '@/shared/themes/Theme';

import type { FlagContainerProps } from './types';

export const Container = styled.View<FlagContainerProps>`
  align-items: center;
  background-color: ${Theme.colors.flagFallback};
  border-color: ${Theme.colors.softBorder};
  border-radius: 9999px;
  border-width: 1px;
  height: ${({ $size }) => $size}px;
  justify-content: center;
  overflow: hidden;
  width: ${({ $size }) => $size}px;
  ${shadow({ elevation: 3, offsetY: 2, opacity: 0.1, radius: 5 })}
`;

export const FlagImage = styled.Image.attrs({
  resizeMode: 'cover',
})`
  height: 100%;
  width: 100%;
`;

export const fallbackIcon = {
  color: Theme.colors.disabled,
  strokeWidth: 1.8,
};
