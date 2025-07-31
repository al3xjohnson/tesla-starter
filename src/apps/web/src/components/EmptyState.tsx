import { LucideIcon } from 'lucide-react';

interface EmptyStateProps {
  icon?: LucideIcon;
  title: string;
  description?: string;
  action?: React.ReactNode;
}

export const EmptyState: React.FC<EmptyStateProps> = ({ 
  icon: Icon, 
  title, 
  description, 
  action 
}) => (
  <div className="text-center py-8">
    {Icon && <Icon className="h-12 w-12 text-muted-foreground mx-auto mb-4" />}
    <p className="text-muted-foreground">{title}</p>
    {description && (
      <p className="text-sm text-muted-foreground mt-2">{description}</p>
    )}
    {action && <div className="mt-4">{action}</div>}
  </div>
);