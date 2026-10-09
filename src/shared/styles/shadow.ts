import { css } from 'styled-components/native';

import { Theme } from '@/shared/themes/Theme';

import type { ShadowOptions } from './types';

export function shadow({ elevation, offsetY, opacity, radius }: ShadowOptions) {
  return css`
    elevation: ${elevation};
    shadow-color: ${Theme.colors.darkNavy};
    shadow-offset: 0px ${offsetY}px;
    shadow-opacity: ${opacity};
    shadow-radius: ${radius}px;
  `;
}
