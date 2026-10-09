import { styled } from 'styled-components/native';

import { Theme } from '@/shared/themes/Theme';

import type { TabLabelState, TabLabelStateProps } from './types';

const INDICATOR_OPACITY: Record<TabLabelState, number> = {
  active: 1,
  inactive: 0,
};

const LABEL_COLOR: Record<TabLabelState, string> = {
  active: Theme.colors.primaryNavy,
  inactive: Theme.colors.disabled,
};

const LABEL_FONT: Record<TabLabelState, string> = {
  active: Theme.fonts.semiBold,
  inactive: Theme.fonts.regular,
};

export const ActiveIndicator = styled.View<TabLabelStateProps>`
  background-color: ${Theme.colors.primaryGreen};
  border-radius: 2px;
  height: 3px;
  margin-top: 4px;
  opacity: ${({ $state }) => INDICATOR_OPACITY[$state]};
  width: 30px;
`;

export const Container = styled.View`
  align-items: center;
  height: 20px;
`;

export const Label = styled.Text<TabLabelStateProps>`
  color: ${({ $state }) => LABEL_COLOR[$state]};
  font-family: ${({ $state }) => LABEL_FONT[$state]};
  font-size: 10px;
  line-height: 13px;
`;
