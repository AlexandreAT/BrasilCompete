import type { TabLabelProps, TabLabelState } from './types';

export function useTabLabel({ focused }: Pick<TabLabelProps, 'focused'>) {
  const state: TabLabelState = focused ? 'active' : 'inactive';

  return { state };
}
