export type TabLabelState = 'active' | 'inactive';

export type TabLabelProps = {
  focused: boolean;
  label: string;
};

export type TabLabelStateProps = {
  $state: TabLabelState;
};
